using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketAssigneeCommand(long Id, string Assignee): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketAssigneeCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketAssigneeCommandHandler> logger)
    : IRequestHandler<UpdateTicketAssigneeCommand>
{
    public async Task Handle(UpdateTicketAssigneeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating executor for ticket {TicketId} to: {NewExecutor}", request.Id, request.Assignee);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for executor update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);

        ticket.ChangeAssignee(request.Assignee);
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated executor for ticket {TicketId}", request.Id);
    }
}