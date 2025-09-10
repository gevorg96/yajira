namespace YetAnotherJira.Models.V1.Responses;

public class V1GetTicketsResponse
{
    public TicketUnit[] Tickets { get; set; }
    
    public int Total { get; set; }
    
    public int Page { get; set; }
    
    public int PageSize { get; set; }
    
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
        
        public DateTimeOffset CreatedAt { get; set; }
    
        public DateTimeOffset UpdatedAt { get; set; }
    }
}