namespace banking_monolithic.Models
{
    public class Loan
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public LoanStatus Status { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    }

    public enum LoanStatus { Pending, Approved, Rejected, Active, Closed }
}
