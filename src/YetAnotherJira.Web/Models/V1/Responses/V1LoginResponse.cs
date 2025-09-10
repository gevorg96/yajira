namespace YetAnotherJira.Models.V1.Responses;

public class V1LoginResponse
{
    public string Token { get; set; }
    public string Username { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
