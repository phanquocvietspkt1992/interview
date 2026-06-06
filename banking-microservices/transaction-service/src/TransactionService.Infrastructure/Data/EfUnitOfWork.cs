using TransactionService.Application.Common;

namespace TransactionService.Infrastructure.Data;

public class EfUnitOfWork(TransactionDbContext db) : IUnitOfWork
{
    public Task CommitAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
