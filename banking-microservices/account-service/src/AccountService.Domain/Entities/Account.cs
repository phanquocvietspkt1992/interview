using AccountService.Domain.Enums;
using AccountService.Domain.Events;
using AccountService.Domain.Exceptions;

namespace AccountService.Domain.Entities;

/// <summary>
/// Account — the Aggregate Root of this bounded context.
///
/// Key DDD rules enforced here:
///   1. All state changes go through methods (no public setters).
///   2. Business rules live inside the aggregate, not in services.
///   3. Domain events are collected inside and dispatched AFTER the DB commit.
///
/// Why Aggregate Root?
///   An aggregate root is the ONLY entry point to a cluster of objects.
///   No other service can touch Account's Balance directly — they must call Deposit/Withdraw.
/// </summary>
public class Account
{
    // ── Identity ───────────────────────────────────────────────────────────
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = default!;
    public Guid CustomerId { get; private set; }

    // ── State ──────────────────────────────────────────────────────────────
    public decimal Balance { get; private set; }
    public AccountType Type { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // ── Domain Events ──────────────────────────────────────────────────────
    // Collected here, dispatched by Infrastructure AFTER SaveChanges().
    // This keeps the aggregate pure — it doesn't know about message buses.
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    // EF Core needs a private/protected parameterless constructor to materialise entities.
    private Account() { }

    // ── Factory Method ─────────────────────────────────────────────────────
    /// <summary>
    /// Always create an Account through this method, never via `new Account()`.
    /// This enforces that every account starts with a valid state.
    /// </summary>
    public static Account Create(Guid customerId, AccountType type, decimal initialDeposit)
    {
        if (initialDeposit < 0)
            throw new DomainException("Initial deposit cannot be negative");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = GenerateAccountNumber(),
            CustomerId = customerId,
            Balance = initialDeposit,
            Type = type,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        account.Raise(new AccountCreatedEvent(account.Id, customerId, initialDeposit));
        return account;
    }

    // ── Business Operations ────────────────────────────────────────────────

    public void Deposit(decimal amount)
    {
        EnsureActive();
        if (amount <= 0) throw new DomainException("Deposit amount must be positive");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        Raise(new MoneyDepositedEvent(Id, amount, Balance));
    }

    public void Withdraw(decimal amount)
    {
        EnsureActive();
        if (amount <= 0) throw new DomainException("Withdrawal amount must be positive");
        if (Balance < amount) throw new InsufficientFundsException(Id, amount, Balance);

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        Raise(new MoneyWithdrawnEvent(Id, amount, Balance));
    }

    public void Freeze(string reason)
    {
        if (Status == AccountStatus.Closed) throw new DomainException("Cannot freeze a closed account");

        Status = AccountStatus.Frozen;
        UpdatedAt = DateTime.UtcNow;

        Raise(new AccountFrozenEvent(Id, reason));
    }

    public void Close()
    {
        EnsureActive();
        if (Balance > 0) throw new DomainException("Cannot close account with remaining balance. Withdraw funds first.");

        Status = AccountStatus.Closed;
        UpdatedAt = DateTime.UtcNow;

        Raise(new AccountClosedEvent(Id));
    }

    // ── Private Helpers ────────────────────────────────────────────────────

    private void EnsureActive()
    {
        if (Status != AccountStatus.Active)
            throw new AccountNotActiveException(Id, Status.ToString());
    }

    private void Raise(DomainEvent e) => _domainEvents.Add(e);

    private static string GenerateAccountNumber()
        => $"ACC{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(10000, 99999)}";
}
