using LoanService.Domain.Enums;
using LoanService.Domain.Events;
using LoanService.Domain.Exceptions;

namespace LoanService.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid AccountId { get; private set; }

    public decimal PrincipalAmount { get; private set; }
    public decimal InterestRate { get; private set; }
    public int TermMonths { get; private set; }
    public decimal OutstandingBalance { get; private set; }

    public LoanStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    public DateTime AppliedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? PaidOffAt { get; private set; }

    public decimal MonthlyPayment => CalculateMonthlyPayment();

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    private Loan() { }

    public static Loan Apply(Guid customerId, Guid accountId, decimal amount, decimal interestRate, int termMonths)
    {
        if (amount <= 0) throw new DomainException("Loan amount must be positive");
        if (interestRate < 0) throw new DomainException("Interest rate cannot be negative");
        if (termMonths <= 0) throw new DomainException("Loan term must be at least 1 month");

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            AccountId = accountId,
            PrincipalAmount = amount,
            InterestRate = interestRate,
            TermMonths = termMonths,
            OutstandingBalance = amount,
            Status = LoanStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        loan.Raise(new LoanAppliedEvent(loan.Id, customerId, amount));
        return loan;
    }

    public void Approve()
    {
        if (Status != LoanStatus.Pending)
            throw new InvalidLoanOperationException("Only pending loans can be approved");

        Status = LoanStatus.Approved;
        ApprovedAt = DateTime.UtcNow;

        Raise(new LoanApprovedEvent(Id, CustomerId, PrincipalAmount));
    }

    public void Reject(string reason)
    {
        if (Status != LoanStatus.Pending)
            throw new InvalidLoanOperationException("Only pending loans can be rejected");

        Status = LoanStatus.Rejected;
        RejectionReason = reason;

        Raise(new LoanRejectedEvent(Id, reason));
    }

    public void Activate()
    {
        if (Status != LoanStatus.Approved)
            throw new InvalidLoanOperationException("Only approved loans can be activated");

        Status = LoanStatus.Active;
    }

    public void MakePayment(decimal amount)
    {
        if (Status != LoanStatus.Active)
            throw new InvalidLoanOperationException("Payments can only be made on active loans");
        if (amount <= 0) throw new DomainException("Payment amount must be positive");
        if (amount > OutstandingBalance) throw new DomainException("Payment exceeds outstanding balance");

        OutstandingBalance -= amount;

        if (OutstandingBalance == 0)
        {
            Status = LoanStatus.PaidOff;
            PaidOffAt = DateTime.UtcNow;
            Raise(new LoanPaidOffEvent(Id));
        }
        else
        {
            Raise(new LoanPaymentMadeEvent(Id, amount, OutstandingBalance));
        }
    }

    private decimal CalculateMonthlyPayment()
    {
        if (InterestRate == 0) return PrincipalAmount / TermMonths;
        var monthlyRate = InterestRate / 100 / 12;
        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), TermMonths);
        return Math.Round(PrincipalAmount * monthlyRate * factor / (factor - 1), 2);
    }

    private void Raise(DomainEvent e) => _domainEvents.Add(e);
}
