using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOutboxAndSaga : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Outbox Messages ────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                EventType = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxMessages", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OutboxMessages_ProcessedAt_RetryCount",
            table: "OutboxMessages",
            columns: new[] { "ProcessedAt", "RetryCount" });

        // ── Transfer Saga States ───────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "TransferSagaStates",
            columns: table => new
            {
                CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FromAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ToAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TransferSagaStates", x => x.CorrelationId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TransferSagaStates_TransactionId",
            table: "TransferSagaStates",
            column: "TransactionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TransferSagaStates");
        migrationBuilder.DropTable(name: "OutboxMessages");
    }
}
