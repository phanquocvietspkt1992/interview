namespace BankingApp.Models;

public enum LoanStatus { Pending, Approved, Rejected, Active, Closed }

public class Loan
{
    public Guid Id { get; set; }
    public decimal Principal { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
