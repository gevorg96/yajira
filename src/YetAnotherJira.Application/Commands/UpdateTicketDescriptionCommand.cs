using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketDescriptionCommand(long Id, string Description) : IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketDescriptionCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketDescriptionCommandHandler> logger)
    : IRequestHandler<UpdateTicketDescriptionCommand>
{
    public async Task Handle(UpdateTicketDescriptionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating description for ticket {TicketId}", request.Id);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for description update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);

        ticket.ChangeDescription(request.Description);
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated description for ticket {TicketId}", request.Id);
    }
}