using Microsoft.EntityFrameworkCore;
using YetAnotherJira.Application.DAL;

namespace YetAnotherJira.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ITicketDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return; 
        }

        var users = new[]
        {
            new UserDal
            {
                Username = "admin",
                Email = "admin@yajira.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            },
            new UserDal
            {
                Username = "john.doe",
                Email = "john.doe@yajira.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            },
            new UserDal
            {
                Username = "jane.smith",
                Email = "jane.smith@yajira.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            },
            new UserDal
            {
                Username = "developer",
                Email = "developer@yajira.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("dev123"),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
