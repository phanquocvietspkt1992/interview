using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class CustomerRepository(IdentityDbContext db, IEventPublisher eventPublisher) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct)
        => db.Customers.FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public Task<List<Customer>> GetAllAsync(CancellationToken ct)
        => db.Customers.Where(c => c.IsActive).ToListAsync(ct);

    public async Task AddAsync(Customer customer, CancellationToken ct)
    {
        await db.Customers.AddAsync(customer, ct);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(customer, ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct)
    {
        db.Customers.Update(customer);
        await db.SaveChangesAsync(ct);
        await DispatchEventsAsync(customer, ct);
    }

    private async Task DispatchEventsAsync(Customer customer, CancellationToken ct)
    {
        foreach (var e in customer.DomainEvents)
            await eventPublisher.PublishAsync(e, ct);
        customer.ClearDomainEvents();
    }
}
