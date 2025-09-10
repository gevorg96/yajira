using YetAnotherJira.Domain.Enums;

namespace YetAnotherJira.Domain.Entities;

public class Ticket
{
    public long Id { get; private set; }

    public TicketStatus Status { get; private set; }

    public TicketPriority Priority { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public string Author { get; private set; }

    public string Assignee { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    
    public long? ParentId { get; private set; }
    
    public Ticket Parent { get; private set; }
    
    public ICollection<TicketRelation> RelatedTickets { get; private set; } = new List<TicketRelation>();
    
    private Ticket() { }
    
    public static Ticket NewTicket(
        TicketPriority priority,
        string header,
        string description,
        string author,
        string assignee,
        long? parentId)
    {
        var now = DateTimeOffset.UtcNow;
        return Create(0, 
            TicketStatus.New,
            priority,
            header,
            description,
            author,
            assignee,
            false, 
            parentId,
            now,
            now);
    }

    public static Ticket Create(
        long id,
        TicketStatus status,
        TicketPriority priority,
        string title,
        string description,
        string author,
        string assignee,
        bool isDeleted,
        long? parentId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Ticket parent = null,
        ICollection<TicketRelation> relatedTickets = null)
    {
        return new Ticket
        {
            Id = id,
            Status = status,
            Priority = priority,
            Title = title,
            Description = description,
            Author = author,
            Assignee = assignee,
            IsDeleted = isDeleted,
            ParentId = parentId,
            Parent = parent,
            RelatedTickets = relatedTickets ?? new List<TicketRelation>(),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
    
    public void ChangeTitle(string title)
    {
        Title = title;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void ChangeDescription(string description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void ChangeAuthor(string author)
    {
        Author = author;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void ChangeAssignee(string assignee)
    {
        Assignee = assignee;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeHeader(string header)
    {
        Title = header;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void ChangeParentId(long? parentId)
    {
        ParentId = parentId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void ChangeStatus(TicketStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePriority(TicketPriority newPriority)
    {
        Priority = newPriority;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}