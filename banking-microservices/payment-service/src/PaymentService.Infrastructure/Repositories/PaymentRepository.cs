using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Common;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class PaymentRepository(PaymentDbContext db, IEventPublisher eventPublisher) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Payment>> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        => db.Payments
             .Where(p => p.AccountId == accountId)
             .OrderByDescending(p => p.CreatedAt)
             .ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        await db.Payments.AddAsync(payment, ct);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(payment, ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct)
    {
        db.Payments.Update(payment);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(payment, ct);
    }

    private async Task DispatchEventsAsync(Payment payment, CancellationToken ct)
    {
        foreach (var e in payment.DomainEvents)
            await eventPublisher.PublishAsync(e, ct);
        payment.ClearDomainEvents();
    }
}
