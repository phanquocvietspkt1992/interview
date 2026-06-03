namespace banking_monolithic.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public Guid FromAccountId { get; set; }
        public Account FromAccount { get; set; } = null!;
        public Guid ToAccountId { get; set; }
        public Account ToAccount { get; set; } = null!;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    }

    public enum PaymentStatus { Pending, Completed, Failed }
}
