using MediatR;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Repositories;

namespace TransactionService.Application.Queries.GetTransactionHistory;

public record GetTransactionHistoryQuery(Guid AccountId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<TransactionDto>>;

public class GetTransactionHistoryQueryHandler(
    ITransactionRepository repository
) : IRequestHandler<GetTransactionHistoryQuery, PagedResult<TransactionDto>>
{
    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionHistoryQuery query, CancellationToken ct)
    {
        var transactions = await repository.GetByAccountIdAsync(query.AccountId, query.Page, query.PageSize, ct);
        var dtos = transactions.Select(t => t.ToDto()).ToList();
        return new PagedResult<TransactionDto>(dtos, query.Page, query.PageSize, dtos.Count);
    }
}
