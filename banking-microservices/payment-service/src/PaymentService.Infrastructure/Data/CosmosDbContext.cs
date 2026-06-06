using Microsoft.Azure.Cosmos;

namespace PaymentService.Infrastructure.Data;

/// <summary>
/// CosmosDB context for PaymentService.
/// PaymentService uses CosmosDB for global distribution, multi-region writes,
/// and schema flexibility — important for cross-border payment networks (SWIFT, SEPA).
///
/// Partition key: /fromAccountId — queries are almost always scoped by account.
/// </summary>
public sealed class CosmosDbContext
{
    private readonly CosmosClient _client;
    private const string DatabaseId = "PaymentServiceDb";
    private const string ContainerId = "Payments";

    public CosmosDbContext(CosmosClient client)
    {
        _client = client;
    }

    public Container Payments => _client.GetContainer(DatabaseId, ContainerId);

    /// <summary>Creates the database and container if they don't exist. Called at startup.</summary>
    public async Task InitializeAsync()
    {
        var dbResponse = await _client.CreateDatabaseIfNotExistsAsync(DatabaseId);
        await dbResponse.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(ContainerId, "/fromAccountId"));
    }
}
