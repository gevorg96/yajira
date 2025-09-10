using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Commands;

public record CreateTicketCommand(
    TicketPriority Priority,
    string Title,
    string Description,
    string Author,
    string Assignee,
    long? Parent): IRequest<long>;

[UsedImplicitly]
internal sealed class CreateTicketCommandHandler(ITicketDbContext dbContext, ILogger<CreateTicketCommandHandler> logger) : IRequestHandler<CreateTicketCommand, long>
{
    public async Task<long> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating new ticket with title: {Title}, author: {Author}, assignee: {Assignee}, parent: {Parent}", 
            request.Title, request.Author, request.Assignee, request.Parent);
        
        TicketDal parentTicket = null;
        if (request.Parent is not null)
        {
            parentTicket = await dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == request.Parent, cancellationToken);
            if (parentTicket is null)
            {
                logger.LogWarning("Parent ticket with id {ParentId} not found for new ticket", request.Parent.Value);
                throw new TicketNotFoundException(request.Parent.Value);
            }
            logger.LogDebug("Found parent ticket with id {ParentId}", request.Parent.Value);
        }
        
        var ticket = Ticket.NewTicket(
            request.Priority,
            request.Title,
            request.Description,
            request.Author,
            request.Assignee,
            parentTicket?.Id);
        
        var ticketDal = TicketMapper.Map(ticket);
        
        var e = dbContext.Tickets.Add(ticketDal);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully created ticket with id {TicketId}", e.Entity.Id);
        return e.Entity.Id;
    }
}