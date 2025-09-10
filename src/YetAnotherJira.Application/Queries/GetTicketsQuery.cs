using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Enums;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Enums;

namespace YetAnotherJira.Application.Queries;

public record GetTicketsQuery(
    SortBy SortBy,
    SortOrder SortOrder,
    int Page,
    int PageSize,
    string Search,
    TicketStatus[] Statuses,
    TicketPriority[] Priorities,
    string[] Authors,
    string[] Assignees) : IRequest<GetTicketsResult>;

public record GetTicketsResult(
    Ticket[] Items,
    int Total
);

[UsedImplicitly]
internal sealed class GetTicketsQueryHandler(ITicketDbContext dbContext) : IRequestHandler<GetTicketsQuery, GetTicketsResult>
{
    public async Task<GetTicketsResult> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Tickets.AsNoTracking().AsQueryable();
        
        if(request.Statuses?.Length > 0)
        {
            query = query.Where(t => request.Statuses.Contains(t.Status));
        }

        if (request.Priorities?.Length > 0)
        {
            query = query.Where(t => request.Priorities.Contains(t.Priority));
        }
        
        if (request.Authors?.Length > 0)
        {
            var authors = request.Authors.ToList();
            query = query.Where(t => authors.Contains(t.Author));
        }
        
        if (request.Assignees?.Length > 0)
        {
            query = query.Where(t => request.Assignees.Contains(t.Assignee));
        }
        
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(t => t.Title.Contains(request.Search) || t.Description.Contains(request.Search));
        }

        var total = await query.CountAsync(cancellationToken: cancellationToken);
        
        query = request.SortBy switch
        {
            SortBy.CreatedAt => request.SortOrder == SortOrder.Asc 
                ? query.OrderBy(t => t.CreatedAt) 
                : query.OrderByDescending(t => t.CreatedAt),
            SortBy.Priority => request.SortOrder == SortOrder.Asc 
                ? query.OrderBy(t => t.Priority) 
                : query.OrderByDescending(t => t.Priority),
            SortBy.Status => request.SortOrder == SortOrder.Asc 
                ? query.OrderBy(t => t.Status) 
                : query.OrderByDescending(t => t.Status),
            _ => query
        };
        
        var res = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        
        return new GetTicketsResult(res.Select(TicketMapper.Map).ToArray(), total);
    }
}