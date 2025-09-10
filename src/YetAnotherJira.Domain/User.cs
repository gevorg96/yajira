namespace YetAnotherJira.Domain;

public class User
{
    public long Id { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private User() { }

    public static User Create(string username, string email, string passwordHash)
    {
        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public static User Create(long id, string username, string email, string passwordHash, DateTimeOffset createdAt, bool isActive)
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = createdAt,
            IsActive = isActive
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
