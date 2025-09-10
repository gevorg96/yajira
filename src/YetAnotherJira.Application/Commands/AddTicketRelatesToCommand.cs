using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record AddTicketRelatesToCommand(long FromTaskId, long ToTaskId, TicketRelationType RelationType): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class AddTicketRelatesToCommandHandler(ITicketDbContext dbContext, ILogger<AddTicketRelatesToCommandHandler> logger)
    : IRequestHandler<AddTicketRelatesToCommand>
{
    public async Task Handle(AddTicketRelatesToCommand request, CancellationToken cancellationToken)
    {
        var fromTaskExists = await dbContext.Tickets.AnyAsync(t => t.Id == request.FromTaskId, cancellationToken);
        var toTaskExists = await dbContext.Tickets.AnyAsync(t => t.Id == request.ToTaskId, cancellationToken);

        if (!fromTaskExists)
        {
            throw new TicketNotFoundException(request.FromTaskId);
        }

        if (!toTaskExists)
        {
            throw new TicketNotFoundException(request.ToTaskId);
        }

        if (request.FromTaskId == request.ToTaskId)
        {
            throw new RelationToItselfException(request.FromTaskId);
        }

        var existingRelation = await dbContext.TicketRelations
            .FirstOrDefaultAsync(tr => tr.FromTaskId == request.FromTaskId 
                                       && tr.ToTaskId == request.ToTaskId 
                                       && tr.RelationType == request.RelationType, 
                cancellationToken);

        if (existingRelation != null)
        {
            throw new RelationAlreadyExistsException(request.FromTaskId, request.ToTaskId);
        }

        var relation = new TicketRelationDal
        {
            FromTaskId = request.FromTaskId,
            ToTaskId = request.ToTaskId,
            RelationType = request.RelationType,
        };
        
        dbContext.TicketRelations.Add(relation);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created relation {RelationType} from task {FromId} to task {ToId}", 
            relation.RelationType, relation.FromTaskId, relation.ToTaskId);
    }
}