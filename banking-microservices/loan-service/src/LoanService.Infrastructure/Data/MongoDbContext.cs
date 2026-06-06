using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace LoanService.Infrastructure.Data;

/// <summary>
/// LoanService uses MongoDB — ideal for loans because:
/// - Loan structures vary by product (personal, mortgage, auto, student)
/// - Nested documents for payment schedules, collateral, terms
/// - Schema evolution without migrations (new fields added without ALTER TABLE)
/// </summary>
public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        // Register Guid as string in BSON for readability
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LoanDb") ?? "mongodb://localhost:27017";
        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        _database = client.GetDatabase(mongoUrl.DatabaseName ?? "LoanServiceDb");

        EnsureIndexes();
    }

    public IMongoCollection<LoanDocument> Loans => _database.GetCollection<LoanDocument>("loans");

    private void EnsureIndexes()
    {
        var indexOptions = new CreateIndexOptions { Background = true };

        Loans.Indexes.CreateOne(new CreateIndexModel<LoanDocument>(
            Builders<LoanDocument>.IndexKeys.Ascending(l => l.CustomerId), indexOptions));

        Loans.Indexes.CreateOne(new CreateIndexModel<LoanDocument>(
            Builders<LoanDocument>.IndexKeys.Ascending(l => l.Status), indexOptions));

        Loans.Indexes.CreateOne(new CreateIndexModel<LoanDocument>(
            Builders<LoanDocument>.IndexKeys
                .Ascending(l => l.CustomerId)
                .Ascending(l => l.Status), indexOptions));
    }
}
