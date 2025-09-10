using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketParentCommand(long Id, long? Parent) : IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketParentCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketParentCommandHandler> logger)
    : IRequestHandler<UpdateTicketParentCommand>
{
    public async Task Handle(UpdateTicketParentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating parent for ticket {TicketId} to: {NewParent}", request.Id, request.Parent);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for parent update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);

        if (request.Parent is not null)
        {
            var parentTicket = await dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == request.Parent, cancellationToken);
            if (parentTicket is null)
            {
                logger.LogWarning("Parent ticket with id {ParentId} not found for ticket {TicketId}", request.Parent.Value, request.Id);
                throw new TicketNotFoundException(request.Parent.Value);
            }
            
            logger.LogDebug("Setting parent ticket {ParentId} for ticket {TicketId}", request.Parent.Value, request.Id);
            ticket.ChangeParentId(parentTicket.Id);
        }
        else
        {
            logger.LogDebug("Removing parent for ticket {TicketId}", request.Id);
            ticket.ChangeParentId(null);
        }
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated parent for ticket {TicketId}", request.Id);
    }
}