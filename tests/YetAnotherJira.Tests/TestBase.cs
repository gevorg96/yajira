using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Services;
using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Infrastructure;
using YetAnotherJira.Infrastructure.Data;

namespace YetAnotherJira.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly TicketDbContext DbContext;
    protected readonly Mock<IJwtService> MockJwtService;

    protected TestBase()
    {
        var services = new ServiceCollection();
        
         
        services.AddDbContext<TicketDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
         
        services.AddLogging(builder => builder.AddConsole());
        
         
        MockJwtService = new Mock<IJwtService>();
        MockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");
        services.AddSingleton(MockJwtService.Object);
        
        // Add application services
        services.AddScoped<ITicketDbContext>(provider => provider.GetRequiredService<TicketDbContext>());
        
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<TicketDbContext>();
        
        // Ensure database is created
        DbContext.Database.EnsureCreated();
        
        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add test users
        var users = new[]
        {
            new UserDal
            {
                Id = 1,
                Username = "admin",
                Email = "admin@yajira.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new UserDal
            {
                Id = 2,
                Username = "user1",
                Email = "user1@yajira.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        DbContext.Users.AddRange(users);

        // Add test tickets
        var tickets = new[]
        {
            new TicketDal
            {
                Id = 1,
                Title = "Test Ticket 1",
                Description = "Test Description 1",
                Author = "admin",
                Assignee = "user1",
                Priority = TicketPriority.High,
                Status = TicketStatus.New,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TicketDal
            {
                Id = 2,
                Title = "Test Ticket 2",
                Description = "Test Description 2",
                Author = "user1",
                Assignee = "admin",
                Priority = TicketPriority.Medium,
                Status = TicketStatus.InProgress,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        DbContext.Tickets.AddRange(tickets);
        DbContext.SaveChanges();
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    protected ILogger<T> GetLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }

    protected void ClearChangeTracker()
    {
        DbContext.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
    }
}
