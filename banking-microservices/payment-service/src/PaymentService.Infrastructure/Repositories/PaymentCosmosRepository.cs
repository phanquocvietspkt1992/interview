using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// CosmosDB implementation of IPaymentRepository.
/// Uses point reads (O(1)) when possible — they are cheaper and faster than queries.
/// </summary>
public class PaymentCosmosRepository(
    CosmosDbContext cosmosDbContext,
    ILogger<PaymentCosmosRepository> logger) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            // Point read requires id + partition key. Without partition key we do a query.
            var query = cosmosDbContext.Payments
                .GetItemLinqQueryable<PaymentDocument>()
                .Where(p => p.Id == id.ToString())
                .ToFeedIterator();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync(ct))
                    return MapToDomain(doc);
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Payment>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        var results = new List<Payment>();
        var query = cosmosDbContext.Payments
            .GetItemLinqQueryable<PaymentDocument>(
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(accountId.ToString()) })
            .Where(p => p.FromAccountId == accountId.ToString())
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            foreach (var doc in await query.ReadNextAsync(ct))
                results.Add(MapToDomain(doc));
        }

        return results;
    }

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        var doc = MapToDocument(payment);
        await cosmosDbContext.Payments.CreateItemAsync(
            doc,
            new PartitionKey(doc.FromAccountId),
            cancellationToken: ct);

        logger.LogDebug("Payment {Id} saved to CosmosDB", payment.Id);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        var doc = MapToDocument(payment);
        await cosmosDbContext.Payments.UpsertItemAsync(
            doc,
            new PartitionKey(doc.FromAccountId),
            cancellationToken: ct);
    }

    private static PaymentDocument MapToDocument(Payment p) => new()
    {
        Id = p.Id.ToString(),
        FromAccountId = p.AccountId.ToString(),
        Reference = p.Reference,
        Amount = p.Amount,
        Currency = p.Currency,
        Network = p.Network.ToString(),
        Status = p.Status.ToString(),
        FailureReason = p.FailureReason,
        Description = p.Description,
        CreatedAt = p.CreatedAt,
        ProcessedAt = p.ProcessedAt,
    };

    private static Payment MapToDomain(PaymentDocument doc)
    {
        // Reconstruct via the factory with dummy values then use reflection for status
        // In production, use a proper reconstitution method on the entity
        var payment = Payment.Initiate(
            Guid.Parse(doc.FromAccountId),
            "N/A",
            "N/A",
            doc.Amount,
            doc.Currency,
            Enum.Parse<PaymentService.Domain.Enums.PaymentNetwork>(doc.Network),
            doc.Description);

        // Apply the stored status
        if (doc.Status == "Processing" || doc.Status == "Completed" || doc.Status == "Failed")
        {
            payment.Process();
            if (doc.Status == "Completed") payment.Complete();
            else if (doc.Status == "Failed") payment.Fail(doc.FailureReason ?? "Unknown");
        }

        return payment;
    }
}
