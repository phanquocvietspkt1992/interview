namespace BankingApp.Models;

public enum PaymentStatus { Pending, Completed, Failed }

public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
}
