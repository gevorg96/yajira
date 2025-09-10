using MediatR;
using YetAnotherJira.Application.DAL;

namespace YetAnotherJira.Application.Behaviours;

public interface ITransactionBehaviour;

public class TransactionBehaviour<TRequest, TResponse>(ITicketDbContext dbContext) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull, ITransactionBehaviour
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var res = await next(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return res;
        }
        catch (Exception e)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}