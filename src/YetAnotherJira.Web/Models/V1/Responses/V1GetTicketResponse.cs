namespace YetAnotherJira.Models.V1.Responses;

public class V1GetTicketResponse
{
    public TicketUnit Data { get; set; }
    
    public class TicketUnit
    {
        public long Id { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public string Assignee { get; set; }
    
        public bool IsDeleted { get; set; }
        
        public TicketUnit Parent { get; set; }
        
        public RelatedTicket[] RelatesTo { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
    
        public DateTimeOffset UpdatedAt { get; set; }
    }
    
    public class RelatedTicket
    {
        public TicketUnit Ticket { get; set; }
        
        public string RelationType { get; set; }
    }
}