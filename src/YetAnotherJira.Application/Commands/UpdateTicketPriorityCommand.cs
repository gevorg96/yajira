using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketPriorityCommand(long Id, TicketPriority Priority): IRequest, ITransactionBehaviour;

[UsedImplicitly]
public class UpdateTicketPriorityCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketPriorityCommandHandler> logger) : IRequestHandler<UpdateTicketPriorityCommand>
{
    public async Task Handle(UpdateTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating priority for ticket {TicketId} to: {NewPriority}", request.Id, request.Priority);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for priority update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);
        ticket.ChangePriority(request.Priority);
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated priority for ticket {TicketId}", request.Id);
    }
}