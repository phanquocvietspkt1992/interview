namespace TransactionService.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Assembly-qualified type name used to deserialize the payload.</summary>
    public string EventType { get; init; } = default!;

    /// <summary>JSON-serialized event payload.</summary>
    public string Payload { get; init; } = default!;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Set once the message has been successfully published.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Last error message if publishing failed.</summary>
    public string? Error { get; set; }

    /// <summary>Number of publish attempts that have failed.</summary>
    public int RetryCount { get; set; }
}
