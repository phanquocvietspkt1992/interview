using TransactionService.Domain.Enums;

namespace TransactionService.Application.DTOs;

public record TransactionDto(
    Guid Id,
    string Reference,
    Guid FromAccountId,
    Guid? ToAccountId,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status,
    string? Description,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record InitiateTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description
);

public record RecordDepositRequest(
    Guid AccountId,
    decimal Amount,
    string? Description
);

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int Total);
