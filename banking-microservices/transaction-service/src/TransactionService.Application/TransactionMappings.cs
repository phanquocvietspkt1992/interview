using TransactionService.Application.DTOs;
using TransactionService.Domain.Entities;

namespace TransactionService.Application;

public static class TransactionMappings
{
    public static TransactionDto ToDto(this Transaction t) => new(
        t.Id,
        t.Reference,
        t.FromAccountId,
        t.ToAccountId,
        t.Amount,
        t.Type,
        t.Status,
        t.Description,
        t.FailureReason,
        t.CreatedAt,
        t.CompletedAt
    );
}
