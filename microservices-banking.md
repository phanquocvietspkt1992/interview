# Microservices Interview Prep — Banking System Simulation

Track each item with `- [x]` when done, `- [ ]` when pending.

---

## Phase 1 — Monolith → Microservice Thinking

**Goal:** Understand the problems that drive the move to microservices.

### Concepts to Review

- [x] What is a Monolith
- [x] Problems of a Monolith
- [x] Why Microservices
- [x] Domain Driven Design (DDD)
- [x] Bounded Context
- [x] Service Boundary

---

### Step 1 — The Monolith (Banking v1)

**Structure:**

```
BankingApp
├── Controllers
├── Services
├── Repositories
└── Database (BankDB)
    ├── Customer
    ├── Account
    ├── Transaction
    ├── Loan
    └── Payment
```

**Deployment:** single binary — `banking-api.exe`  
**Database:** single DB — `BankDB`

---

#### Example Flow: Transfer Money (A → B, $100)

**Request:**
```
POST /transfer
```

**Controller:**
```csharp
public class TransferController
{
    private readonly TransferService _service;

    public async Task<IActionResult> Transfer(TransferRequest req)
    {
        await _service.Transfer(req);
        return Ok();
    }
}
```

**Service:**
```csharp
public class TransferService
{
    private readonly AccountRepository _repo;

    public async Task Transfer(TransferRequest req)
    {
        var sender   = await _repo.Get(req.From);
        var receiver = await _repo.Get(req.To);

        if (sender.Balance < req.Amount)
            throw new Exception("Insufficient funds");

        sender.Balance   -= req.Amount;
        receiver.Balance += req.Amount;

        await _repo.Save();
    }
}
```

**Repository:**
```csharp
public class AccountRepository
{
    private readonly DbContext _db;

    public async Task<Account> Get(string id)
        => await _db.Accounts.FindAsync(id);

    public Task Save()
        => _db.SaveChangesAsync();
}
```

---

### Step 2 — Problems of the Monolith

- [ ] **Scaling bottleneck** — must scale the entire app even if only Transfer is under load
- [ ] **Tight coupling** — one bug in Loan logic can crash the whole app
- [ ] **Slow deployments** — a 1-line fix requires redeploying everything
- [ ] **Team friction** — multiple teams edit the same codebase, merge conflicts everywhere
- [ ] **Technology lock-in** — entire app is stuck on one tech stack
- [ ] **Database coupling** — all domains share one DB; schema changes are risky

---

### Step 3 — Why Microservices?

- [ ] Each service is **independently deployable**
- [ ] Each service **scales independently** (e.g., scale only the Transfer service)
- [ ] Teams own their own service end-to-end
- [ ] Services can use **different tech stacks** where appropriate
- [ ] Failure is **isolated** — a crash in Loan service doesn't kill Accounts

---

### Step 4 — Domain Driven Design (DDD) Basics

- [ ] **Domain** — the business problem you are solving (Banking)
- [ ] **Bounded Context** — a boundary within which a model is valid and consistent
- [ ] **Aggregate** — a cluster of objects treated as a unit (e.g., `Account` + `Balance`)
- [ ] **Entity** — has a unique identity (e.g., `Customer`, `Account`)
- [ ] **Value Object** — no identity, defined by its value (e.g., `Money`, `IBAN`)
- [ ] **Domain Event** — something that happened (e.g., `MoneyTransferred`)
- [ ] **Repository** — abstraction for loading/saving aggregates

---

### Step 5 — Identifying Bounded Contexts in Banking

Map each domain concept to a service boundary:

| Bounded Context     | Owns                              | Service Name          |
|---------------------|-----------------------------------|-----------------------|
| Identity & Access   | Customer, Login, KYC              | `identity-service`    |
| Account Management  | Account, Balance                  | `account-service`     |
| Transactions        | Transfer, History, Statement      | `transaction-service` |
| Loans               | Loan Application, Repayment       | `loan-service`        |
| Notifications       | Email, SMS, Push                  | `notification-service`|
| Payments            | External payments, SWIFT, ACH     | `payment-service`     |

- [ ] Each service has its **own database** — no shared DB
- [ ] Services communicate via **API calls** or **events**, not direct DB joins
- [ ] A customer in `identity-service` is referenced by ID only in other services

---

### Phase 1 Review Checklist

- [x] Can explain the monolith structure and its trade-offs
- [x] Can name at least 5 problems with a monolith
- [x] Can explain what DDD is and why it matters for microservices
- [x] Can draw the Banking bounded contexts on a whiteboard
- [x] Can explain why each service must own its own database
- [x] Can walk through the Transfer flow in the monolith and identify what breaks at scale

---

## Phase 2 — Service Communication

> *(Coming next)*

- [ ] Synchronous — REST, gRPC
- [ ] Asynchronous — Message Bus (RabbitMQ, Azure Service Bus, Kafka)
- [ ] When to use sync vs async
- [ ] API Gateway pattern

---

## Phase 3 — Data Management

> *(Coming next)*

- [ ] Database per service
- [ ] Eventual consistency
- [ ] Saga pattern (Choreography vs Orchestration)
- [ ] Outbox pattern

---

## Phase 4 — Resilience & Fault Tolerance

> *(Coming next)*

- [ ] Circuit Breaker (Polly)
- [ ] Retry & Timeout policies
- [ ] Bulkhead isolation
- [ ] Health checks

---

## Phase 5 — Security

> *(Coming next)*

- [ ] JWT / OAuth2 / OpenID Connect
- [ ] API Gateway authentication
- [ ] Service-to-service auth (mTLS, service tokens)

---

## Phase 6 — Observability

> *(Coming next)*

- [ ] Structured logging (Serilog)
- [ ] Distributed tracing (OpenTelemetry, Jaeger)
- [ ] Metrics (Prometheus, Grafana)
- [ ] Correlation IDs across services

---

## Phase 7 — Deployment & Infrastructure

> *(Coming next)*

- [ ] Docker & containerization
- [ ] Kubernetes basics (pods, services, deployments)
- [ ] CI/CD pipeline per service
- [ ] Environment configuration (secrets, config maps)

---

## Phase 8 — Interview Questions Bank

> *(Will be filled as phases complete)*

- [ ] "How would you break a monolith into microservices?"
- [ ] "How do you handle a transaction that spans multiple services?"
- [ ] "What happens when Service B is down and Service A calls it?"
- [ ] "How do you prevent data inconsistency across services?"
- [ ] "How do you debug a request that touches 5 services?"
