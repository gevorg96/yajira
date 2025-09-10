using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketTitleCommand(long Id, string Title): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketTitleCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketTitleCommandHandler> logger)
    : IRequestHandler<UpdateTicketTitleCommand>
{
    public async Task Handle(UpdateTicketTitleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating title for ticket {TicketId} to: {NewTitle}", request.Id, request.Title);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for title update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);

        ticket.ChangeHeader(request.Title);
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated title for ticket {TicketId}", request.Id);
    }
}