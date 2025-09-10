using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketStatusCommand(long Id, TicketStatus Status): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketStatusCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketStatusCommandHandler> logger)
    : IRequestHandler<UpdateTicketStatusCommand>
{
    public async Task Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating status for ticket {TicketId} to: {NewStatus}", request.Id, request.Status);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for status update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);

        ticket.ChangeStatus(request.Status);
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated status for ticket {TicketId}", request.Id);
    }
}