using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Entities;
using YetAnotherJira.Models.V1.Responses;

namespace YetAnotherJira.Controllers.Mappers;

public static class TicketMapper
{
    public static V1GetTicketResponse.TicketUnit MapSingle(Ticket model)
    {
        if (model is null)
        {
            return null;
        }

        var parent = model.Parent != null ? new V1GetTicketResponse.TicketUnit
        {
            Id = model.Parent.Id,
            Status = model.Parent.Status.ToString(),
            Priority = model.Parent.Priority.ToString(),
            Title = model.Parent.Title,
            Description = model.Parent.Description,
            Author = model.Parent.Author,
            Assignee = model.Parent.Assignee,
            IsDeleted = model.Parent.IsDeleted,
            Parent = null, 
            RelatesTo = [], 
            CreatedAt = model.Parent.CreatedAt,
            UpdatedAt = model.Parent.UpdatedAt
        } : null;

        var relatedTickets = model.RelatedTickets?.Select(rt => new V1GetTicketResponse.RelatedTicket
        {
            Ticket = new V1GetTicketResponse.TicketUnit
            {
                Id = rt.RelatedTicket.Id,
                Status = rt.RelatedTicket.Status.ToString(),
                Priority = rt.RelatedTicket.Priority.ToString(),
                Title = rt.RelatedTicket.Title,
                Description = rt.RelatedTicket.Description,
                Author = rt.RelatedTicket.Author,
                Assignee = rt.RelatedTicket.Assignee,
                IsDeleted = rt.RelatedTicket.IsDeleted,
                Parent = null, 
                RelatesTo = [],
                CreatedAt = rt.RelatedTicket.CreatedAt,
                UpdatedAt = rt.RelatedTicket.UpdatedAt
            },
            RelationType = rt.RelationType.ToString()
        }).ToArray() ?? [];

        return new V1GetTicketResponse.TicketUnit
        {
            Id = model.Id,
            Status = model.Status.ToString(),
            Priority = model.Priority.ToString(),
            Title = model.Title,
            Description = model.Description,
            Author = model.Author,
            Assignee = model.Assignee,
            IsDeleted = model.IsDeleted,
            Parent = parent,
            RelatesTo = relatedTickets,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }

    public static V1GetTicketsResponse.TicketUnit MapQuery(Ticket model)
    {
        if (model is null)
        {
            return null;
        }

        return new V1GetTicketsResponse.TicketUnit
        {
            Id = model.Id,
            Status = model.Status.ToString(),
            Priority = model.Priority.ToString(),
            Title = model.Title,
            Description = model.Description,
            Author = model.Author,
            Assignee = model.Assignee,
            IsDeleted = model.IsDeleted,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}