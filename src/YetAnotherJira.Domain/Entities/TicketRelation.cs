using YetAnotherJira.Domain.Enums;

namespace YetAnotherJira.Domain.Entities;

public class TicketRelation
{
    public Ticket RelatedTicket { get; private set; }
    
    public TicketRelationType RelationType { get; private set; }
    
    private TicketRelation() { }
    
    public static TicketRelation Create(Ticket relatedTicket, TicketRelationType relationType)
    {
        return new TicketRelation
        {
            RelatedTicket = relatedTicket,
            RelationType = relationType
        };
    }
}
