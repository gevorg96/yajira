using YetAnotherJira.Domain.Enums;

namespace YetAnotherJira.Application.DAL;

public class TicketRelationDal
{
    public long FromTaskId { get; set; }
    
    public TicketDal FromTask { get; set; }

    public long ToTaskId { get; set; }
    
    public TicketDal ToTask { get; set; }

    public TicketRelationType RelationType { get; set; }
}