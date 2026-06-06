using LoanService.Domain.Entities;
using LoanService.Domain.Enums;
using LoanService.Domain.Repositories;
using LoanService.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace LoanService.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation — note how we use the document model (LoanDocument)
/// for storage and map to/from the domain entity (Loan).
/// Domain entity stays pure; MongoDB concerns stay in Infrastructure.
/// </summary>
public class LoanMongoRepository(
    MongoDbContext context,
    ILogger<LoanMongoRepository> logger) : ILoanRepository
{
    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cursor = await context.Loans.FindAsync(
            Builders<LoanDocument>.Filter.Eq(l => l.Id, id), cancellationToken: ct);
        var doc = await cursor.FirstOrDefaultAsync(ct);
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task<List<Loan>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var cursor = await context.Loans.FindAsync(
            Builders<LoanDocument>.Filter.Eq(l => l.CustomerId, customerId), cancellationToken: ct);
        var docs = await cursor.ToListAsync(ct);
        return docs.Select(MapToDomain).ToList();
    }

    public async Task AddAsync(Loan loan, CancellationToken ct = default)
    {
        var doc = MapToDocument(loan);
        await context.Loans.InsertOneAsync(doc, cancellationToken: ct);
        logger.LogDebug("Loan {Id} inserted into MongoDB", loan.Id);
    }

    public async Task UpdateAsync(Loan loan, CancellationToken ct = default)
    {
        var doc = MapToDocument(loan);
        await context.Loans.ReplaceOneAsync(
            Builders<LoanDocument>.Filter.Eq(l => l.Id, loan.Id),
            doc,
            new ReplaceOptions { IsUpsert = false },
            ct);
    }

    private static LoanDocument MapToDocument(Loan loan) => new()
    {
        Id = loan.Id,
        CustomerId = loan.CustomerId,
        AccountId = loan.AccountId,
        PrincipalAmount = loan.PrincipalAmount,
        InterestRate = loan.InterestRate,
        TermMonths = loan.TermMonths,
        OutstandingBalance = loan.OutstandingBalance,
        Status = loan.Status.ToString(),
        RejectionReason = loan.RejectionReason,
        AppliedAt = loan.AppliedAt,
        ApprovedAt = loan.ApprovedAt,
        PaidOffAt = loan.PaidOffAt,
    };

    private static Loan MapToDomain(LoanDocument doc)
    {
        // Reconstitute via the factory, then apply stored state
        var loan = Loan.Apply(doc.CustomerId, doc.AccountId,
            doc.PrincipalAmount, doc.InterestRate, doc.TermMonths);

        if (doc.Status == "Approved" || doc.Status == "Active" ||
            doc.Status == "PaidOff" || doc.Status == "Rejected")
        {
            if (doc.Status == "Approved" || doc.Status == "Active" || doc.Status == "PaidOff")
                loan.Approve();
            else if (doc.Status == "Rejected")
                loan.Reject(doc.RejectionReason ?? "Unknown");
        }

        loan.ClearDomainEvents(); // reconstitution should not re-raise events
        return loan;
    }
}
