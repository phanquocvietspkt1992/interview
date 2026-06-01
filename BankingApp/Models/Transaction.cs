namespace BankingApp.Models;

public enum TransactionType { Debit, Credit, Transfer }

public class Transaction
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    // For transfers: the other side
    public Guid? RelatedAccountId { get; set; }
}
