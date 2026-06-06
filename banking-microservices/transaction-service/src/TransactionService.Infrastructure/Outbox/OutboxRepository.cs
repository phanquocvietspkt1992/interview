using TransactionService.Application.Common;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Outbox;

public class OutboxRepository(TransactionDbContext db) : IOutboxRepository
{
    public async Task AddAsync(string eventType, string payload, CancellationToken ct = default)
    {
        var message = new OutboxMessage
        {
            EventType = eventType,
            Payload = payload,
        };
        await db.OutboxMessages.AddAsync(message, ct);
        // NOTE: SaveChanges is NOT called here — the caller must SaveChanges in the same unit of work
        // to achieve atomicity (Transaction + OutboxMessage in one DB transaction)
    }
}
