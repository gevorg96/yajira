using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record DeleteTicketRelatesToCommand(long Id, long[] RelatesTo): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class DeleteTicketRelatesToCommandHandler(ITicketDbContext dbContext, ILogger<DeleteTicketRelatesToCommandHandler> logger)
    : IRequestHandler<DeleteTicketRelatesToCommand>
{
    public async Task Handle(DeleteTicketRelatesToCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting relations for ticket {TicketId} to tickets: [{RelatedTickets}]", 
            request.Id, string.Join(", ", request.RelatesTo));
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for relation deletion", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var deletedRelationsCount = 0;
        foreach (var relatedId in request.RelatesTo)
        {
            var relations = await dbContext.TicketRelations
                .Where(tr => (tr.FromTaskId == request.Id && tr.ToTaskId == relatedId) ||
                            (tr.FromTaskId == relatedId && tr.ToTaskId == request.Id))
                .ToListAsync(cancellationToken);

            if (relations.Count > 0)
            {
                logger.LogDebug("Deleting {RelationCount} relations between tickets {TicketId} and {RelatedId}", 
                    relations.Count, request.Id, relatedId);
                dbContext.TicketRelations.RemoveRange(relations);
                deletedRelationsCount += relations.Count;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully deleted {DeletedCount} relations for ticket {TicketId}", 
            deletedRelationsCount, request.Id);
    }
}