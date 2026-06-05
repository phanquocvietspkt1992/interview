using MediatR;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Exceptions;
using TransactionService.Domain.Repositories;

namespace TransactionService.Application.Queries.GetTransaction;

public record GetTransactionQuery(Guid TransactionId) : IRequest<TransactionDto>;

public class GetTransactionQueryHandler(
    ITransactionRepository repository
) : IRequestHandler<GetTransactionQuery, TransactionDto>
{
    public async Task<TransactionDto> Handle(GetTransactionQuery query, CancellationToken ct)
    {
        var tx = await repository.GetByIdAsync(query.TransactionId, ct)
            ?? throw new TransactionNotFoundException(query.TransactionId);

        return tx.ToDto();
    }
}
