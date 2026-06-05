namespace AccountService.Domain.Events;

public record AccountCreatedEvent(
    Guid AccountId,
    Guid CustomerId,
    decimal InitialDeposit
) : DomainEvent;

public record MoneyDepositedEvent(
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter
) : DomainEvent;

public record MoneyWithdrawnEvent(
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter
) : DomainEvent;

public record AccountClosedEvent(
    Guid AccountId
) : DomainEvent;

public record AccountFrozenEvent(
    Guid AccountId,
    string Reason
) : DomainEvent;
