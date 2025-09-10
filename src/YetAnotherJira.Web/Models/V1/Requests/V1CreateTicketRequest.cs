namespace YetAnotherJira.Models.V1.Requests;

public class V1CreateTicketRequest
{
    public string Priority { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public string Author { get; set; }
    
    public string Assignee { get; set; }
    
    public long? Parent { get; set; }
}