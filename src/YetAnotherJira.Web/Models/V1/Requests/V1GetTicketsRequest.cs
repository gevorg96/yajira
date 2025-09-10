namespace YetAnotherJira.Models.V1.Requests;

public class V1GetTicketsRequest
{
    public string SortBy { get; set; }

    public string SortOrder { get; set; }
    
    public int Page { get; set; }
    
    public int PageSize { get; set; }
    
    public string Search { get; set; }
    
    public string[] Statuses { get; set; }
    
    public string[] Priorities { get; set; }
    
    public string[] Authors { get; set; }
    
    public string[] Assignees { get; set; }
}