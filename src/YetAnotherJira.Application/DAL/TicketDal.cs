using System.ComponentModel.DataAnnotations;
using YetAnotherJira.Domain.Enums;

namespace YetAnotherJira.Application.DAL;

public class TicketDal
{
    public long Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    
    [MaxLength(2000)]
    public string Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Author { get; set; }
    
    [MaxLength(100)]
    public string Assignee { get; set; }
    
    public TicketStatus Status { get; set; } = TicketStatus.New;
    
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public long? ParentTaskId { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public TicketDal ParentTask { get; set; }
    
    public ICollection<TicketDal> SubTasks { get; set; } = new List<TicketDal>();
    
    public ICollection<TicketRelationDal> OutgoingRelations { get; set; } = new List<TicketRelationDal>();
    
    public ICollection<TicketRelationDal> IncomingRelations { get; set; } = new List<TicketRelationDal>();
}