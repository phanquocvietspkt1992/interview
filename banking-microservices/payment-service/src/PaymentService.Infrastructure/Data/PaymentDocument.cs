using Newtonsoft.Json;

namespace PaymentService.Infrastructure.Data;

/// <summary>
/// CosmosDB document model for a Payment.
/// CosmosDB requires a lowercase "id" property and partition key property at the top level.
/// We use a separate document type to avoid polluting the domain entity with CosmosDB concerns.
/// </summary>
public class PaymentDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    [JsonProperty("fromAccountId")]
    public string FromAccountId { get; set; } = default!;

    [JsonProperty("toAccountId")]
    public string? ToAccountId { get; set; }

    [JsonProperty("transactionId")]
    public string? TransactionId { get; set; }

    [JsonProperty("reference")]
    public string Reference { get; set; } = default!;

    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = default!;

    [JsonProperty("network")]
    public string Network { get; set; } = default!;

    [JsonProperty("status")]
    public string Status { get; set; } = default!;

    [JsonProperty("failureReason")]
    public string? FailureReason { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [JsonProperty("_type")]
    public string Type { get; set; } = "Payment";
}
