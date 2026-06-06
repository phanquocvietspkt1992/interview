using Cassandra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Data;

/// <summary>
/// NotificationService uses Cassandra — ideal for:
/// - Very high write throughput (millions of notifications per day)
/// - Time-series data: partitioned by customerId + createdAt for fast range queries
/// - No single point of failure: Cassandra is inherently distributed
///
/// Data model: PRIMARY KEY ((customer_id), created_at DESC, notification_id)
/// - Partition key: customer_id — all of a customer's notifications on one node
/// - Clustering: created_at DESC — most-recent-first by default
/// </summary>
public sealed class CassandraContext : IDisposable
{
    private readonly ICluster _cluster;
    public ISession Session { get; }
    public PreparedStatement InsertStmt { get; private set; } = default!;
    public PreparedStatement SelectByCustomerStmt { get; private set; } = default!;
    public PreparedStatement SelectByIdStmt { get; private set; } = default!;
    public PreparedStatement UpdateStatusStmt { get; private set; } = default!;

    public CassandraContext(IConfiguration configuration, ILogger<CassandraContext> logger)
    {
        var host = configuration["Cassandra__Host"] ?? "localhost";
        var port = int.Parse(configuration["Cassandra__Port"] ?? "9042");
        var keyspace = configuration["Cassandra__Keyspace"] ?? "notification_service";

        _cluster = Cluster.Builder()
            .AddContactPoint(host)
            .WithPort(port)
            .WithReconnectionPolicy(new ExponentialReconnectionPolicy(1000, 60000))
            .Build();

        // Create keyspace first (connect without keyspace)
        var bootstrapSession = _cluster.Connect();
        bootstrapSession.Execute($@"
            CREATE KEYSPACE IF NOT EXISTS {keyspace}
            WITH replication = {{'class': 'SimpleStrategy', 'replication_factor': 1}}
            AND durable_writes = true");
        bootstrapSession.Dispose();

        Session = _cluster.Connect(keyspace);
        InitializeSchema(keyspace);
        PrepareStatements();

        logger.LogInformation("Cassandra connected to keyspace {Keyspace}", keyspace);
    }

    private void InitializeSchema(string keyspace)
    {
        Session.Execute($@"
            CREATE TABLE IF NOT EXISTS {keyspace}.notifications (
                customer_id   UUID,
                created_at    TIMESTAMP,
                id            UUID,
                channel       TEXT,
                status        TEXT,
                subject       TEXT,
                body          TEXT,
                sent_at       TIMESTAMP,
                PRIMARY KEY ((customer_id), created_at, id)
            ) WITH CLUSTERING ORDER BY (created_at DESC, id ASC)
              AND default_time_to_live = 7776000");  // 90 days TTL

        // Materialized-view-style index via a secondary table for id lookups
        Session.Execute($@"
            CREATE TABLE IF NOT EXISTS {keyspace}.notifications_by_id (
                id          UUID PRIMARY KEY,
                customer_id UUID,
                created_at  TIMESTAMP
            )");
    }

    private void PrepareStatements()
    {
        InsertStmt = Session.Prepare(@"
            INSERT INTO notifications (customer_id, created_at, id, channel, status, subject, body)
            VALUES (?, ?, ?, ?, ?, ?, ?)");

        SelectByCustomerStmt = Session.Prepare(@"
            SELECT * FROM notifications
            WHERE customer_id = ?
            LIMIT 50");

        SelectByIdStmt = Session.Prepare(@"
            SELECT customer_id, created_at FROM notifications_by_id WHERE id = ?");

        UpdateStatusStmt = Session.Prepare(@"
            UPDATE notifications SET status = ?, sent_at = ?
            WHERE customer_id = ? AND created_at = ? AND id = ?");
    }

    public void Dispose()
    {
        Session.Dispose();
        _cluster.Dispose();
    }
}
