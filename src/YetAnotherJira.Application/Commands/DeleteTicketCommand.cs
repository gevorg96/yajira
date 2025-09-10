using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record DeleteTicketCommand(long Id) : IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class DeleteTicketCommandHandler(ITicketDbContext dbContext, ILogger<DeleteTicketCommandHandler> logger) : IRequestHandler<DeleteTicketCommand>
{
    public async Task Handle(DeleteTicketCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting ticket with id {TicketId}", request.Id);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for deletion", request.Id);
            throw new TicketNotFoundException(request.Id);
        }
        
        var ticket = TicketMapper.Map(ticketDal);
        ticket.Delete();
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully deleted ticket with id {TicketId}", request.Id);
    }
}