using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketAuthorCommand(long Id, string Author): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketAuthorCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketAuthorCommandHandler> logger)
    : IRequestHandler<UpdateTicketAuthorCommand>
{
    public async Task Handle(UpdateTicketAuthorCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating author for ticket {TicketId} to: {NewAuthor}", request.Id, request.Author);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for author update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }

        var ticket = TicketMapper.Map(ticketDal);

        ticket.ChangeAuthor(request.Author);
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated author for ticket {TicketId}", request.Id);
    }
}