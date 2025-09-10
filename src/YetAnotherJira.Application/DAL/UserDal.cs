using System.ComponentModel.DataAnnotations;

namespace YetAnotherJira.Application.DAL;

public class UserDal
{
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public bool IsActive { get; set; } = true;
}
