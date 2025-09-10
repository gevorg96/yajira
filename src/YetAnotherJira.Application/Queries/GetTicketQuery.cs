using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Entities;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Application.Queries;

public record GetTicketQuery(long Id): IRequest<Ticket>;

[UsedImplicitly]
internal sealed class GetTicketQueryHandler(ITicketDbContext dbContext) : IRequestHandler<GetTicketQuery, Ticket>
{
    public async Task<Ticket> Handle(GetTicketQuery request, CancellationToken cancellationToken)
    {
        var res = await dbContext.Tickets
            .AsNoTracking()
            .Include(x => x.ParentTask)
            .Include(x => x.OutgoingRelations)
            .ThenInclude(x => x.ToTask)
            .Include(x => x.IncomingRelations)
            .ThenInclude(x => x.FromTask)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        
        if (res is null)
        {
            throw new TicketNotFoundException(request.Id);
        }

        return TicketMapper.Map(res);
    }
}