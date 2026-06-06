using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LoanService.Infrastructure.Data;

/// <summary>
/// MongoDB document model — demonstrates nested document capability.
/// PaymentHistory is stored as a subdocument array, avoiding a JOIN that relational DBs would need.
/// </summary>
[BsonIgnoreExtraElements]
public class LoanDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid CustomerId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid AccountId { get; set; }

    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = "Pending";
    public string? RejectionReason { get; set; }

    public DateTime AppliedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidOffAt { get; set; }

    // Nested array — no JOIN required, atomically updated
    public List<LoanPaymentRecord> PaymentHistory { get; set; } = [];
}

public class LoanPaymentRecord
{
    [BsonRepresentation(BsonType.String)]
    public Guid PaymentId { get; set; }

    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public decimal BalanceAfterPayment { get; set; }
}
