namespace YetAnotherJira.Models.V1.Requests;

public class V1AddTicketRelatesToRequest
{
    public long[] RelatesTo { get; set; }
    
    public string RelationType { get; set; } = "RelatedTo";
}