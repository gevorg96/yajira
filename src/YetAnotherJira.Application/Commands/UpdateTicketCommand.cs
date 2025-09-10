using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.Behaviours;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record UpdateTicketCommand(
    long Id,
    string Title,
    string Description,
    string Author,
    string Assignee,
    long? Parent
    ): IRequest, ITransactionBehaviour;

[UsedImplicitly]
internal sealed class UpdateTicketCommandHandler(ITicketDbContext dbContext, ILogger<UpdateTicketCommandHandler> logger) : IRequestHandler<UpdateTicketCommand>
{
    public async Task Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating ticket with id {TicketId}. Fields to update: Title={Title}, Description={Description}, Author={Author}, Assignee={Assignee}, Parent={Parent}", 
            request.Id, request.Title != null, request.Description != null, request.Author != null, request.Assignee != null, request.Parent);
        
        var ticketDal = await dbContext.SelectForUpdate(request.Id, cancellationToken);

        if (ticketDal is null)
        {
            logger.LogWarning("Ticket with id {TicketId} not found for update", request.Id);
            throw new TicketNotFoundException(request.Id);
        }
        
        var ticket = TicketMapper.Map(ticketDal);
        
        if (request.Title is not null)
        {
            ticket.ChangeHeader(request.Title);
        }

        if (request.Description is not null)
        {
            ticket.ChangeDescription(request.Description);
        }

        if (request.Author is not null)
        {
            ticket.ChangeAuthor(request.Author);
        }

        if (request.Assignee is not null)
        {
            ticket.ChangeAssignee(request.Assignee);
        }

        if (request.Parent is not null)
        {
            var parentTicket = await dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == request.Parent, cancellationToken);
            if (parentTicket is null)
            {
                logger.LogWarning("Parent ticket with id {ParentId} not found for ticket {TicketId}", request.Parent.Value, request.Id);
                throw new TicketNotFoundException(request.Parent.Value);
            }
            logger.LogDebug("Setting parent ticket {ParentId} for ticket {TicketId}", request.Parent.Value, request.Id);
            
            ticket.ChangeParentId(request.Parent.Value);
        }
        else
        {
            ticket.ChangeParentId(null);
        }
        
        ticketDal = TicketMapper.Map(ticket);
        dbContext.Tickets.Update(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully updated ticket with id {TicketId}", request.Id);
    }
}