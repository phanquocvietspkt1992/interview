namespace BankingApp.Models;

public enum AccountType { Checking, Savings }
public enum AccountStatus { Active, Frozen, Closed }

public class Account
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
