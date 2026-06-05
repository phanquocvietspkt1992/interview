namespace LoanService.Domain.Events;

public record LoanAppliedEvent(Guid LoanId, Guid CustomerId, decimal Amount) : DomainEvent;
public record LoanApprovedEvent(Guid LoanId, Guid CustomerId, decimal Amount) : DomainEvent;
public record LoanRejectedEvent(Guid LoanId, string Reason) : DomainEvent;
public record LoanPaymentMadeEvent(Guid LoanId, decimal PaymentAmount, decimal RemainingBalance) : DomainEvent;
public record LoanPaidOffEvent(Guid LoanId) : DomainEvent;
