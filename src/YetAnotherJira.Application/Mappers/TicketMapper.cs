using YetAnotherJira.Application.DAL;
using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Entities;

namespace YetAnotherJira.Application.Mappers;

public static class TicketMapper
{
    public static Ticket Map(TicketDal model)
    {
        if (model is null)
            return null;

        var parent = model.ParentTask != null 
            ? Ticket.Create(
                model.ParentTask.Id,
                model.ParentTask.Status,
                model.ParentTask.Priority,
                model.ParentTask.Title,
                model.ParentTask.Description,
                model.ParentTask.Author,
                model.ParentTask.Assignee,
                model.ParentTask.IsDeleted,
                model.ParentTask.ParentTaskId,
                model.ParentTask.CreatedAt,
                model.ParentTask.UpdatedAt)
            : null;

        var relatedTickets = model.OutgoingRelations?
            .Where(r => r.ToTask != null)
            .Select(r => TicketRelation.Create(
                Ticket.Create(
                    r.ToTask.Id,
                    r.ToTask.Status,
                    r.ToTask.Priority,
                    r.ToTask.Title,
                    r.ToTask.Description,
                    r.ToTask.Author,
                    r.ToTask.Assignee,
                    r.ToTask.IsDeleted,
                    r.ToTask.ParentTaskId,
                    r.ToTask.CreatedAt,
                    r.ToTask.UpdatedAt),
                r.RelationType))
            .ToList() ?? new List<TicketRelation>();

        return Ticket.Create(
            model.Id,
            model.Status,
            model.Priority,
            model.Title,
            model.Description,
            model.Author,
            model.Assignee,
            model.IsDeleted,
            model.ParentTaskId,
            model.CreatedAt,
            model.UpdatedAt,
            parent,
            relatedTickets
        );
    }

    public static TicketDal Map(Ticket model)
    {
        return model is null
            ? null
            : new TicketDal
            {
                Id = model.Id,
                Status = model.Status,
                Priority = model.Priority,
                Title = model.Title,
                Description = model.Description,
                Author = model.Author,
                Assignee = model.Assignee,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                IsDeleted = model.IsDeleted,
                ParentTaskId = model.ParentId
            };
    }
}