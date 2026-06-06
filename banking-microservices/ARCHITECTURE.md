# Full Microservices Workflow

---

## 1. High-Level Architecture

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│                                   CLIENT                                          │
│                         (Browser / Mobile / Postman)                             │
└─────────────────────────────────┬────────────────────────────────────────────────┘
                                  │  HTTP
                                  ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              API GATEWAY  :5000                                  │
│                           (YARP Reverse Proxy)                                   │
│                                                                                  │
│  /api/identity/**  →  identity-service                                           │
│  /api/accounts/**  →  account-service                                            │
│  /api/transactions/** →  transaction-service                                     │
│  /api/loans/**     →  loan-service                                               │
│  /api/payments/**  →  payment-service                                            │
│  /api/notifications/** →  notification-service                                   │
└──────┬──────────┬──────────┬──────────┬──────────┬──────────┬───────────────────┘
       │          │          │          │          │          │
       ▼          ▼          ▼          ▼          ▼          ▼
  ┌─────────┐ ┌────────┐ ┌──────────┐ ┌──────┐ ┌────────┐ ┌──────────────┐
  │Identity │ │Account │ │Transaction│ │ Loan │ │Payment │ │Notification  │
  │Service  │ │Service │ │ Service  │ │Service│ │Service │ │  Service     │
  │  :5002  │ │ :5001  │ │  :5003   │ │ :5004 │ │ :5006  │ │    :5005     │
  └────┬────┘ └───┬────┘ └────┬─────┘ └──┬───┘ └───┬────┘ └──────┬───────┘
       │          │           │           │          │             │
       ▼          ▼           ▼           ▼          ▼             ▼
  ┌─────────┐ ┌────────┐ ┌──────────┐ ┌──────┐ ┌────────┐ ┌──────────────┐
  │  MSSQL  │ │ Oracle │ │  MSSQL   │ │Mongo │ │Cosmos  │ │  Cassandra   │
  │         │ │   XE   │ │ +Outbox  │ │  DB  │ │  DB    │ │              │
  └─────────┘ └────────┘ └──────────┘ └──────┘ └────────┘ └──────────────┘
```

---

## 2. Messaging Infrastructure

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              RABBITMQ  :5672                                     │
│                         (Transactional Commands/Events)                          │
│                                                                                  │
│  Queues:                                                                         │
│  ┌──────────────────────────┐   ┌──────────────────────────┐                    │
│  │  transfer-saga-queue     │   │  account-debit-queue      │                    │
│  │  transfer-completed-queue│   │  account-credit-queue     │                    │
│  │  transfer-failed-queue   │   │  account-reverse-queue    │                    │
│  └──────────────────────────┘   │  payment-process-queue    │                    │
│                                  └──────────────────────────┘                    │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                               KAFKA  :9092                                       │
│                         (Audit Log / Event Streaming)                            │
│                                                                                  │
│  Topics:                                                                         │
│  ┌──────────────────────────────┐  ┌──────────────────────────────┐             │
│  │  transaction-completed       │  │  transaction-failed          │             │
│  │  (retained, replayable)      │  │  (retained, replayable)      │             │
│  └──────────────────────────────┘  └──────────────────────────────┘             │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Transfer Saga — Happy Path (Full Detail)

```
CLIENT                API-GW         TransactionSvc         AccountSvc          PaymentSvc
  │                     │                   │                    │                   │
  │  POST /transfers     │                   │                    │                   │
  │─────────────────────►│                   │                    │                   │
  │                     │  POST /transfers   │                    │                   │
  │                     │──────────────────►│                    │                   │
  │                     │                   │                    │                   │
  │                     │         ┌─────────────────────┐        │                   │
  │                     │         │  DB Transaction      │        │                   │
  │                     │         │  INSERT Transaction  │        │                   │
  │                     │         │  INSERT OutboxMsg    │        │                   │
  │                     │         │  (atomic SaveChanges)│        │                   │
  │                     │         └─────────────────────┘        │                   │
  │                     │                   │                    │                   │
  │   202 Accepted       │                   │                    │                   │
  │   { status: Pending }│                   │                    │                   │
  │◄─────────────────────│◄──────────────────│                    │                   │
  │                     │                   │                    │                   │
  │                     │         ┌──────────────────┐           │                   │
  │                     │         │  OutboxProcessor  │           │                   │
  │                     │         │  (every 5 secs)   │           │                   │
  │                     │         │  reads OutboxMsg  │           │                   │
  │                     │         └────────┬─────────┘           │                   │
  │                     │                  │ publishes            │                   │
  │                     │                  │ TransferSagaStarted  │                   │
  │                     │                  ▼                      │                   │
  │                     │         ┌──────────────────┐           │                   │
  │                     │         │  TransferSaga     │           │                   │
  │                     │         │  State: Initial   │           │                   │
  │                     │         │  → DebitingAcct   │           │                   │
  │                     │         └────────┬─────────┘           │                   │
  │                     │                  │ DebitAccountCommand  │                   │
  │                     │                  │─────────────────────►│                   │
  │                     │                  │                      │  Withdraw(amount)  │
  │                     │                  │                      │  SaveChanges       │
  │                     │                  │                      │                   │
  │                     │                  │   AccountDebitedEvent│                   │
  │                     │                  │◄─────────────────────│                   │
  │                     │                  │                      │                   │
  │                     │         ┌──────────────────┐           │                   │
  │                     │         │  TransferSaga     │           │                   │
  │                     │         │  DebitingAcct     │           │                   │
  │                     │         │  → ProcessingPmt  │           │                   │
  │                     │         └────────┬─────────┘           │                   │
  │                     │                  │ ProcessPaymentCommand│                   │
  │                     │                  │──────────────────────────────────────────►│
  │                     │                  │                      │  Payment.Process() │
  │                     │                  │                      │  Payment.Complete()│
  │                     │                  │                      │  CosmosDB insert   │
  │                     │                  │                      │                   │
  │                     │                  │     PaymentProcessedEvent                 │
  │                     │                  │◄──────────────────────────────────────────│
  │                     │                  │                      │                   │
  │                     │         ┌──────────────────┐           │                   │
  │                     │         │  TransferSaga     │           │                   │
  │                     │         │  ProcessingPmt    │           │                   │
  │                     │         │  → CreditingAcct  │           │                   │
  │                     │         └────────┬─────────┘           │                   │
  │                     │                  │ CreditAccountCommand │                   │
  │                     │                  │─────────────────────►│                   │
  │                     │                  │                      │  Deposit(amount)   │
  │                     │                  │                      │  SaveChanges       │
  │                     │                  │                      │                   │
  │                     │                  │   AccountCreditedEvent                   │
  │                     │                  │◄─────────────────────│                   │
  │                     │                  │                      │                   │
  │                     │         ┌──────────────────┐           │                   │
  │                     │         │  TransferSaga     │           │                   │
  │                     │         │  CreditingAcct    │           │                   │
  │                     │         │  → Completed ✓    │           │                   │
  │                     │         └────────┬─────────┘           │                   │
  │                     │                  │ Transaction.Complete()                    │
  │                     │                  │ UPDATE status = Completed                 │
  │                     │                  │                      │                   │
  │                     │                  │ TransactionCompletedAuditEvent            │
  │                     │                  │──────────────────────────────────────────►
  │                     │                  │                          KAFKA            │
  │                     │                  │                    topic: transaction-completed
```

---

## 4. Transfer Saga — Compensation Flow (Payment Fails)

```
TransactionSvc (Saga)        AccountSvc             PaymentSvc
       │                         │                       │
       │  DebitAccountCommand     │                       │
       │────────────────────────►│                       │
       │                         │  Withdraw(amount) ✓   │
       │   AccountDebitedEvent    │                       │
       │◄────────────────────────│                       │
       │                         │                       │
       │  ProcessPaymentCommand   │                       │
       │──────────────────────────────────────────────►  │
       │                         │                       │  ~5% random fail
       │                         │                       │  Payment.Fail(reason)
       │      PaymentFailedEvent  │                       │  CosmosDB insert
       │◄──────────────────────────────────────────────  │
       │                         │                       │
  ┌────┴──────────────┐          │                       │
  │  Saga: Compensate │          │                       │
  │  ProcessingPmt    │          │                       │
  │  → CompensatDebit │          │                       │
  └────┬──────────────┘          │                       │
       │  ReverseDebitCommand     │                       │
       │────────────────────────►│                       │
       │                         │  Deposit(amount) ✓    │  ← reversal
       │  AccountDebitReversedEv  │  (re-credit the funds)│
       │◄────────────────────────│                       │
       │                         │                       │
  ┌────┴──────────────┐          │                       │
  │  Saga: Failed ✗   │          │                       │
  │  Transaction.Fail()│          │                      │
  └────┬──────────────┘          │                       │
       │ TransactionFailedAuditEvent                      │
       │──────────────────────────────────────────────────────► KAFKA
       │                                           topic: transaction-failed
```

---

## 5. Notification Flow (Kafka Consumer)

```
                    KAFKA
        topic: transaction-completed / transaction-failed
                         │
                         │  KafkaNotificationConsumer
                         │  (BackgroundService, manual commit)
                         ▼
               ┌──────────────────┐
               │ NotificationSvc  │
               │                  │
               │ Create Notif     │
               │ entity           │
               └────────┬─────────┘
                        │
                        ▼
               ┌──────────────────┐
               │    CASSANDRA     │
               │                  │
               │ TABLE:           │
               │ notifications    │
               │ PK: (customer_id,│
               │  created_at DESC,│
               │  id)             │
               │                  │
               │ INSERT batch:    │
               │ ├ notifications  │
               │ └ notifications_ │
               │    by_id (lookup)│
               └──────────────────┘
```

---

## 6. Outbox Pattern (Detail)

```
┌────────────────────────────────────────────────────────────────┐
│                    TransactionService                           │
│                                                                 │
│  POST /transfers                                                │
│       │                                                         │
│       ▼                                                         │
│  InitiateTransferCommandHandler                                 │
│       │                                                         │
│       │  ┌─────────────────────────────────────────────────┐   │
│       │  │          ONE DB TRANSACTION (atomic)             │   │
│       │  │                                                   │   │
│       │  │  transactionRepo.AddAsync(transaction)  ──┐      │   │
│       │  │  outboxRepo.AddAsync(sagaEvent JSON)    ──┤      │   │
│       │  │  unitOfWork.CommitAsync()               ──┘      │   │
│       │  │         ↓                                         │   │
│       │  │  Transactions table:  [Pending]                   │   │
│       │  │  OutboxMessages table: [Unprocessed]              │   │
│       │  └─────────────────────────────────────────────────┘   │
│       │                                                         │
│       │  return 202 Accepted immediately                        │
│       │                                                         │
│  ┌────▼───────────────────────────────────────────────────┐    │
│  │  OutboxProcessor  (BackgroundService)                   │    │
│  │                                                         │    │
│  │  loop every 5s:                                         │    │
│  │    SELECT TOP 50 FROM OutboxMessages                    │    │
│  │    WHERE ProcessedAt IS NULL AND RetryCount < 3         │    │
│  │    ORDER BY CreatedAt                                   │    │
│  │                                                         │    │
│  │    foreach message:                                     │    │
│  │      deserialize(EventType, Payload)                    │    │
│  │      IPublishEndpoint.Publish(event)  ──► RabbitMQ      │    │
│  │      message.ProcessedAt = UtcNow                       │    │
│  │      SaveChanges                                        │    │
│  │                                                         │    │
│  │    on failure:                                          │    │
│  │      message.RetryCount++                               │    │
│  │      message.Error = ex.Message                         │    │
│  │      (retry next cycle, max 3 attempts)                 │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## 7. Saga State Machine (States & Transitions)

```
                     ┌───────────┐
                     │  INITIAL  │
                     └─────┬─────┘
                           │ TransferSagaStarted
                           │ → publish DebitAccountCommand
                           ▼
                  ┌─────────────────┐
                  │  DEBITING_ACCT  │◄──────────────────────────────┐
                  └────────┬────────┘                               │
                           │                                        │
           ┌───────────────┴──────────────┐                        │
           │ AccountDebitedEvent           │ AccountDebitFailedEvent │
           │ → publish ProcessPaymentCmd  │ → no compensation needed│
           ▼                               ▼                        │
  ┌─────────────────┐             ┌──────────────┐                 │
  │ PROCESSING_PMT  │             │    FAILED ✗   │                 │
  └────────┬────────┘             └──────────────┘                 │
           │                                                        │
  ┌────────┴──────────────────┐                                     │
  │ PaymentProcessedEvent      │ PaymentFailedEvent                  │
  │ → publish CreditAcctCmd   │ → publish ReverseDebitCmd          │
  ▼                            ▼                                    │
  ┌─────────────────┐  ┌──────────────────┐                        │
  │  CREDITING_ACCT │  │ COMPENSATING_DEB │────────────────────────┘
  └────────┬────────┘  └──────────────────┘
           │                   ↑
           │ AccountCreditedEv  AccountDebitReversedEvent
           ▼
  ┌──────────────────┐
  │  COMPLETED ✓     │
  └──────────────────┘

  Each state is persisted in SQL Server (TransferSagaStates table).
  Survives service restarts — saga resumes from last known state.
```

---

## 8. Polyglot Persistence

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          DATABASE PER SERVICE                                    │
│                    (no shared DB, no cross-service JOINs)                        │
└─────────────────────────────────────────────────────────────────────────────────┘

  IdentityService                   AccountService
  ┌──────────────┐                  ┌──────────────────────────┐
  │    MSSQL     │                  │       Oracle XE           │
  │              │                  │                          │
  │ Customers    │                  │ Accounts                 │
  │ Users        │                  │ AccountNumber (unique)   │
  └──────────────┘                  │ Balance (precision 18,4) │
                                    │ Status, Type             │
                                    └──────────────────────────┘

  TransactionService                LoanService
  ┌──────────────────────────┐      ┌─────────────────────────────────┐
  │    MSSQL                 │      │   MongoDB                       │
  │                          │      │                                 │
  │ Transactions             │      │ loans collection {              │
  │ OutboxMessages           │      │   _id: UUID,                   │
  │ TransferSagaStates       │      │   customerId, accountId,       │
  │ (saga persisted in SQL   │      │   principal, interestRate,     │
  │  for ACID + pessimistic  │      │   paymentHistory: [            │
  │  concurrency)            │      │     { paymentId, amount,       │
  └──────────────────────────┘      │       paidAt, balance }        │
                                    │   ]                             │
                                    │ }   ← nested docs, no JOIN     │
                                    └─────────────────────────────────┘

  PaymentService                    NotificationService
  ┌──────────────────────────┐      ┌─────────────────────────────────┐
  │   CosmosDB               │      │   Cassandra                     │
  │                          │      │                                 │
  │ Container: Payments      │      │ TABLE notifications             │
  │ Partition: /fromAccountId│      │   PRIMARY KEY (                 │
  │                          │      │     (customer_id),              │
  │ {                        │      │     created_at DESC,            │
  │   id: "...",             │      │     notification_id             │
  │   fromAccountId: "...",  │      │   )                             │
  │   amount, status,        │      │                                 │
  │   network, currency      │      │ TTL: 90 days (auto-expiry)      │
  │ }                        │      │ Write throughput: unlimited     │
  └──────────────────────────┘      └─────────────────────────────────┘
```

---

## 9. Clean Architecture Layers (per service)

```
  ┌────────────────────────────────────────────────────┐
  │                    API Layer                        │
  │  Controllers, Program.cs, Middleware                │
  │  ─ maps HTTP → MediatR commands/queries             │
  └──────────────────────┬─────────────────────────────┘
                         │ depends on ↓
  ┌────────────────────────────────────────────────────┐
  │                Application Layer                    │
  │  Commands, Queries, DTOs, IUnitOfWork,              │
  │  IOutboxRepository, IEventPublisher                 │
  │  ─ orchestrates use cases, no framework deps        │
  └──────────────────────┬─────────────────────────────┘
                         │ depends on ↓
  ┌────────────────────────────────────────────────────┐
  │                  Domain Layer                       │
  │  Entities (Aggregate Roots), Value Objects,         │
  │  Domain Events, Repository Interfaces               │
  │  ─ pure C#, zero NuGet dependencies                 │
  └──────────────────────┬─────────────────────────────┘
                         │ implemented by ↓
  ┌────────────────────────────────────────────────────┐
  │               Infrastructure Layer                  │
  │  EF Core / MongoDB / CosmosDB / Cassandra,          │
  │  MassTransit consumers, Kafka publisher,            │
  │  OutboxProcessor, Saga state machine                │
  │  ─ all external dependencies live here only         │
  └────────────────────────────────────────────────────┘

  Dependency Rule: outer layers → inner layers only.
  Domain never references EF Core, MassTransit, or Kafka.
```

---

## 10. Full Docker Compose Topology

```
                        ┌─────────────────────────────────────────────┐
                        │               docker network                  │
                        │                                              │
  :15672 ─────────────► │  rabbitmq          :5672  (AMQP)            │
  :8090  ─────────────► │  kafka-ui                                    │
  :9092  ─────────────► │  kafka             :29092 (internal)         │
                        │  zookeeper         :2181                     │
                        │                                              │
  :1435  ─────────────► │  identity-db    (MSSQL)                     │
  :1521  ─────────────► │  account-db     (Oracle XE, linux/amd64)    │
  :1436  ─────────────► │  transaction-db  (MSSQL)                    │
  :27017 ─────────────► │  loan-db        (MongoDB)                   │
  :9042  ─────────────► │  notification-db (Cassandra)                │
  :8081  ─────────────► │  payment-db     (CosmosDB vnext-preview)    │
                        │                                              │
  :5000  ─────────────► │  api-gateway                                │
  :5001  ─────────────► │  account-service                            │
  :5002  ─────────────► │  identity-service                           │
  :5003  ─────────────► │  transaction-service                        │
  :5004  ─────────────► │  loan-service                               │
  :5005  ─────────────► │  notification-service                       │
  :5006  ─────────────► │  payment-service                            │
                        └─────────────────────────────────────────────┘

  Startup order (health-check gated):
  [dbs + brokers healthy] → [microservices start] → [api-gateway starts]
```
