namespace TransactionService.Application.Common;

/// <summary>
/// Application layer abstraction for writing to the Outbox.
/// The Application layer defines the interface; Infrastructure provides the EF Core implementation.
/// This preserves Clean Architecture layering (Application → Domain, not Application → Infrastructure).
/// </summary>
public interface IOutboxRepository
{
    Task AddAsync(string eventType, string payload, CancellationToken ct = default);
}
