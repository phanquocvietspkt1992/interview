


namespace banking_monolithic.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();


    }


    public enum AccountType
    {
        Checking,
        Savings,
        Credit
    }

    public enum AccountStatus
    {
        Active,
        Closed,
        Frozen
    }
}
