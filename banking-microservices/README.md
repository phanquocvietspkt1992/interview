# Banking Microservices

A production-grade **.NET 9** microservices system built for senior developer interview preparation. Demonstrates every standard pattern — Saga, Outbox, CQRS, Clean Architecture, polyglot persistence, and dual-broker messaging — through a realistic banking domain.

---

## Architecture Overview

```
                        ┌─────────────────┐
                        │   API Gateway   │  :5000  (YARP)
                        └────────┬────────┘
                                 │
          ┌──────────────────────┼──────────────────────┐
          │                      │                       │
 ┌────────▼────────┐   ┌────────▼────────┐   ┌────────▼────────┐
 │ IdentityService │   │ AccountService  │   │TransactionService│
 │   MSSQL         │   │   Oracle XE     │   │  MSSQL + Outbox  │
 │   :5002         │   │   :5001         │   │  + Saga + Kafka  │
 └─────────────────┘   └─────────────────┘   └─────────────────┘
                                                        │
          ┌─────────────────────────────────────────────┤
          │                      │                       │
 ┌────────▼────────┐   ┌────────▼────────┐   ┌────────▼────────┐
 │  LoanService    │   │PaymentService   │   │NotificationSvc  │
 │   MongoDB       │   │  CosmosDB       │   │ Cassandra+Kafka  │
 │   :5004         │   │   :5006         │   │   :5005         │
 └─────────────────┘   └─────────────────┘   └─────────────────┘
```

### Messaging Infrastructure

```
RabbitMQ (MassTransit)          Kafka
─────────────────────           ─────
Transactional commands          Append-only audit/event stream
Service-to-service sagas        Fan-out to multiple consumers
Messages deleted on consume     Messages retained (replayable)

account-debit-queue       →     transaction-completed  (topic)
account-credit-queue      →     transaction-failed     (topic)
account-reverse-queue
payment-process-queue
transfer-saga-queue
```

---

## Services

| Service | Port | Database | Purpose |
|---|---|---|---|
| **API Gateway** | 5000 | — | YARP reverse proxy, single entry point |
| **IdentityService** | 5002 | MSSQL | Customers, authentication, JWT |
| **AccountService** | 5001 | Oracle XE | Bank accounts, balance management |
| **TransactionService** | 5003 | MSSQL | Transfers, Outbox, Saga orchestrator |
| **LoanService** | 5004 | MongoDB | Loan applications and repayments |
| **PaymentService** | 5006 | CosmosDB | External payment processing |
| **NotificationService** | 5005 | Cassandra | Customer notifications |

---

## Key Patterns Implemented

### 1. Outbox Pattern

**Problem:** After saving a `Transfer` to the database, if the RabbitMQ publish fails (network blip, broker restart), the Saga never starts. The transaction is orphaned — neither completed nor failed.

**Solution:** Write `Transaction` + `OutboxMessage` in a single `SaveChangesAsync()` call. An `OutboxProcessor` BackgroundService polls every 5 seconds, publishes unpublished messages via MassTransit, and marks them `ProcessedAt`. Survives service crashes.

```
Client → POST /transfers
           │
           ▼
┌──────────────────────────────────┐
│  DB Transaction (atomic)         │
│  ┌────────────────────────────┐  │
│  │ INSERT INTO Transactions   │  │
│  │ INSERT INTO OutboxMessages │  │
│  └────────────────────────────┘  │
└──────────────────────────────────┘
           │
     (5 seconds later)
           │
           ▼
    OutboxProcessor
    publishes TransferSagaStarted → RabbitMQ
```

**Key files:**
- [`TransactionService.Infrastructure/Outbox/OutboxMessage.cs`](transaction-service/src/TransactionService.Infrastructure/Outbox/OutboxMessage.cs)
- [`TransactionService.Infrastructure/Outbox/OutboxProcessor.cs`](transaction-service/src/TransactionService.Infrastructure/Outbox/OutboxProcessor.cs)
- [`TransactionService.Infrastructure/Outbox/OutboxRepository.cs`](transaction-service/src/TransactionService.Infrastructure/Outbox/OutboxRepository.cs)
- [`TransactionService.Application/Common/IUnitOfWork.cs`](transaction-service/src/TransactionService.Application/Common/IUnitOfWork.cs)

---

### 2. Saga Pattern — Orchestration

The `TransferSagaMachine` (MassTransit `StateMachine<TransferSagaState>`) coordinates the full transfer flow. State is persisted in SQL Server — survives restarts mid-flow.

**Happy path:**
```
TransferSagaStarted
    → DebitAccountCommand       ──► AccountService
    ← AccountDebitedEvent
    → ProcessPaymentCommand     ──► PaymentService
    ← PaymentProcessedEvent
    → CreditAccountCommand      ──► AccountService
    ← AccountCreditedEvent
    → [Transaction.Complete()]
    → TransactionCompletedAuditEvent ──► Kafka
```

**Compensation flow (payment fails):**
```
PaymentFailedEvent
    → ReverseDebitCommand       ──► AccountService
    ← AccountDebitReversedEvent
    → [Transaction.Fail()]
    → TransactionFailedAuditEvent ──► Kafka
```

**States:** `Initial → DebitingAccount → ProcessingPayment → CreditingAccount → Completed`
Compensation: `ProcessingPayment → CompensatingDebit → Failed`

**Key files:**
- [`TransactionService.Infrastructure/Sagas/TransferSagaMachine.cs`](transaction-service/src/TransactionService.Infrastructure/Sagas/TransferSagaMachine.cs)
- [`TransactionService.Infrastructure/Sagas/TransferSagaState.cs`](transaction-service/src/TransactionService.Infrastructure/Sagas/TransferSagaState.cs)
- [`TransactionService.Infrastructure/Sagas/SagaCompletionConsumers.cs`](transaction-service/src/TransactionService.Infrastructure/Sagas/SagaCompletionConsumers.cs)

---

### 3. RabbitMQ via MassTransit

Used for **transactional, service-to-service messaging**. Each consumer publishes a result event that drives the next saga step.

| Queue | Consumer | Published Event |
|---|---|---|
| `account-debit-queue` | `DebitAccountConsumer` | `AccountDebitedEvent` / `AccountDebitFailedEvent` |
| `account-credit-queue` | `CreditAccountConsumer` | `AccountCreditedEvent` |
| `account-reverse-debit-queue` | `ReverseDebitConsumer` | `AccountDebitReversedEvent` |
| `payment-process-queue` | `ProcessPaymentConsumer` | `PaymentProcessedEvent` / `PaymentFailedEvent` |
| `transfer-saga-queue` | `TransferSagaMachine` | (drives saga state) |

**Key files:**
- [`AccountService.Infrastructure/Messaging/Consumers/`](account-service/src/AccountService.Infrastructure/Messaging/Consumers/)
- [`PaymentService.Infrastructure/Messaging/Consumers/ProcessPaymentConsumer.cs`](payment-service/src/PaymentService.Infrastructure/Messaging/Consumers/ProcessPaymentConsumer.cs)

---

### 4. Kafka — Event Streaming

Used for **audit log and fan-out** to downstream consumers. Unlike RabbitMQ, messages are retained and replayable.

| Topic | Producer | Consumer |
|---|---|---|
| `transaction-completed` | `KafkaAuditPublisher` (TransactionService) | `KafkaNotificationConsumer` (NotificationService) |
| `transaction-failed` | `KafkaAuditPublisher` (TransactionService) | `KafkaNotificationConsumer` (NotificationService) |

**Why two brokers?**
- RabbitMQ: routing, acknowledgement, request/response patterns — message is deleted after consumption
- Kafka: durable log, multiple independent consumers, time-travel replay — NotificationService can be down for hours and catch up

**Key files:**
- [`TransactionService.Infrastructure/Messaging/KafkaAuditPublisher.cs`](transaction-service/src/TransactionService.Infrastructure/Messaging/KafkaAuditPublisher.cs)
- [`NotificationService.Infrastructure/Messaging/KafkaNotificationConsumer.cs`](notification-service/src/NotificationService.Infrastructure/Messaging/KafkaNotificationConsumer.cs)

---

### 5. CQRS + Clean Architecture

Every service follows the same layering:

```
API  ──►  Application (Commands/Queries via MediatR)
               │
          Domain (Entities, Value Objects, Domain Events)
               │
      Infrastructure (EF Core / MongoDB / CosmosDB / Cassandra, MassTransit, Kafka)
```

**Commands** mutate state, raise domain events, write to outbox.
**Queries** return DTOs directly from the data store.
No anemic domain model — all business rules live inside aggregates.

---

### 6. Polyglot Persistence

Each service owns its database — no shared schemas, no cross-service joins.

| Service | Database | Rationale |
|---|---|---|
| IdentityService | **MSSQL** | Relational integrity for user data, JWT tokens |
| AccountService | **Oracle XE** | Enterprise standard in banking; supports COBOL mainframe migrations |
| TransactionService | **MSSQL** | ACID transactions required for Outbox + Saga state atomicity |
| LoanService | **MongoDB** | Flexible schema for varied loan products; nested `PaymentHistory` documents avoid JOINs |
| PaymentService | **CosmosDB** | Multi-region writes, global distribution for cross-border payments |
| NotificationService | **Cassandra** | Massive write throughput; time-series partitioned by `customer_id + created_at` |

---

### 7. Integration Events (BuildingBlocks.Messaging)

Shared contracts between services — the messaging API surface:

```csharp
// Saga commands (orchestrator → services)
record DebitAccountCommand(Guid CorrelationId, Guid TransactionId, Guid AccountId, decimal Amount);
record CreditAccountCommand(Guid CorrelationId, Guid TransactionId, Guid AccountId, decimal Amount);
record ReverseDebitCommand(Guid CorrelationId, Guid TransactionId, Guid AccountId, decimal Amount);
record ProcessPaymentCommand(Guid CorrelationId, Guid TransactionId, ...);

// Response events (services → saga)
record AccountDebitedEvent(...)
record AccountDebitFailedEvent(...)
record AccountCreditedEvent(...)
record PaymentProcessedEvent(...)
record PaymentFailedEvent(...)

// Kafka audit events
record TransactionCompletedAuditEvent(...)
record TransactionFailedAuditEvent(...)
```

---

## Getting Started

### Prerequisites

- Docker Desktop
- .NET 9 SDK

### Run Infrastructure Only

```bash
# Start all infrastructure (databases + brokers)
docker compose up rabbitmq kafka zookeeper kafka-ui \
    identity-db transaction-db account-db loan-db notification-db payment-db -d
```

> **Note:** Oracle XE (`account-db`) takes ~2–3 minutes to be ready on first start.

### Run All Services

```bash
docker compose up --build
```

### Useful URLs

| Service | URL |
|---|---|
| API Gateway | http://localhost:5000 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |
| Kafka UI | http://localhost:8090 |
| CosmosDB Explorer | https://localhost:8081/_explorer/index.html |
| TransactionService Swagger | http://localhost:5003/swagger |
| AccountService Swagger | http://localhost:5001/swagger |
| LoanService Swagger | http://localhost:5004/swagger |

---

## Transfer Flow — End to End

```bash
# 1. Create two accounts (AccountService)
POST http://localhost:5001/api/accounts
{ "customerId": "...", "type": "Checking", "initialDeposit": 1000 }

# 2. Initiate a transfer (TransactionService)
POST http://localhost:5003/api/transactions/transfer
{
  "fromAccountId": "...",
  "toAccountId": "...",
  "amount": 250.00,
  "description": "Rent payment"
}

# Response is immediate — transaction is Pending
# Background: OutboxProcessor → RabbitMQ → Saga starts
# Watch in RabbitMQ UI: queues filling and draining
# Watch in Kafka UI: transaction-completed topic
```

**Compensation flow** (PaymentService has ~5% random failure rate — just re-run to trigger):
```
AccountService debits → PaymentService fails → AccountService reverses debit
→ Transaction marked Failed → Kafka: transaction-failed
→ NotificationService creates failure notification in Cassandra
```

---

## Project Structure

```
banking-microservices/
├── src/
│   └── BuildingBlocks/
│       └── BuildingBlocks.Messaging/      # Shared integration event contracts
│           └── IntegrationEvents/
│               └── TransferIntegrationEvents.cs
│
├── identity-service/                      # MSSQL
├── account-service/                       # Oracle XE + RabbitMQ consumers
├── transaction-service/                   # MSSQL + Outbox + Saga + Kafka
├── loan-service/                          # MongoDB
├── payment-service/                       # CosmosDB + RabbitMQ consumer
├── notification-service/                  # Cassandra + Kafka consumer
├── api-gateway/                           # YARP
└── docker-compose.yml
```

Each service follows identical layering:
```
ServiceName.API/           Controllers, Program.cs
ServiceName.Application/   Commands, Queries, DTOs (MediatR)
ServiceName.Domain/        Entities, Events, Repositories (interfaces)
ServiceName.Infrastructure/ EF Core / MongoDB / Cosmos / Cassandra, Consumers
```

---

## Interview Topics This Project Covers

| Topic | Where |
|---|---|
| Outbox Pattern (dual-write problem) | `TransactionService.Infrastructure/Outbox/` |
| Saga Orchestration vs Choreography | `TransactionService.Infrastructure/Sagas/` |
| Compensating transactions | `TransferSagaMachine` + `ReverseDebitConsumer` |
| Polyglot persistence | 5 different databases across 6 services |
| CQRS | Every service: separate Command/Query handlers |
| Clean Architecture / DDD | Domain aggregates, domain events, repository pattern |
| Unit of Work pattern | `IUnitOfWork` + `EfUnitOfWork` |
| RabbitMQ vs Kafka trade-offs | Different brokers for different use cases |
| CosmosDB partition design | `/fromAccountId` partition key choice |
| Cassandra time-series modeling | `PRIMARY KEY ((customer_id), created_at DESC)` |
| MongoDB document design | Nested `PaymentHistory` vs relational JOIN |
| Oracle in enterprise banking | `AccountService` using `Oracle.EntityFrameworkCore` |
| MassTransit StateMachine | Persisted saga state, idempotent consumers |
| BackgroundService pattern | `OutboxProcessor`, `KafkaNotificationConsumer` |
| Integration testing strategy | Each service owns its DB — test independently |

---

## Tech Stack

| Technology | Version | Usage |
|---|---|---|
| .NET | 9.0 | All services |
| MassTransit | 8.3.6 | RabbitMQ abstraction + saga state machine |
| Confluent.Kafka | 2.6.0 | Kafka producer/consumer |
| Entity Framework Core | 9.0.5 | MSSQL (Identity, Transaction) |
| Oracle.EntityFrameworkCore | 9.21.130 | Oracle XE (Account) |
| MongoDB.Driver | 2.28.0 | MongoDB (Loan) |
| Microsoft.Azure.Cosmos | 3.41.0 | CosmosDB (Payment) |
| CassandraCSharpDriver | 3.22.0 | Cassandra (Notification) |
| MediatR | 14.1.0 | CQRS command/query dispatch |
| YARP | Latest | API Gateway reverse proxy |
