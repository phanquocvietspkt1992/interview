using LoanService.Application.Common;
using LoanService.Domain.Entities;
using LoanService.Domain.Repositories;
using LoanService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanService.Infrastructure.Repositories;

public class LoanRepository(LoanDbContext db, IEventPublisher eventPublisher) : ILoanRepository
{
    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Loans.FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<List<Loan>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct)
        => db.Loans.Where(l => l.CustomerId == customerId).ToListAsync(ct);

    public async Task AddAsync(Loan loan, CancellationToken ct)
    {
        await db.Loans.AddAsync(loan, ct);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(loan, ct);
    }

    public async Task UpdateAsync(Loan loan, CancellationToken ct)
    {
        db.Loans.Update(loan);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(loan, ct);
    }

    private async Task DispatchEventsAsync(Loan loan, CancellationToken ct)
    {
        foreach (var e in loan.DomainEvents)
            await eventPublisher.PublishAsync(e, ct);
        loan.ClearDomainEvents();
    }
}
