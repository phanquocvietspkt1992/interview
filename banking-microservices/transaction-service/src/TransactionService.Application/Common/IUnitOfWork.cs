namespace TransactionService.Application.Common;

/// <summary>
/// Unit of Work abstraction — coordinates saving multiple repositories in a single DB transaction.
/// This keeps Application layer ignorant of EF Core while enabling atomic multi-repo writes.
/// </summary>
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct = default);
}
