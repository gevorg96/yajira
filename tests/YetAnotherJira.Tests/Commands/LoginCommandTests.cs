using Microsoft.EntityFrameworkCore;
using Moq;
using YetAnotherJira.Application.Commands;

namespace YetAnotherJira.Tests.Commands;

public class LoginCommandTests : TestBase
{
    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnUserAndToken()
    {
        var handler = new LoginCommandHandler(DbContext, MockJwtService.Object, GetLogger<LoginCommandHandler>());
        var command = new LoginCommand("admin", "admin123");
        
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be("admin");
        result.User.Email.Should().Be("admin@yajira.com");
        result.Token.Should().Be("test-jwt-token");
        
        MockJwtService.Verify(x => x.GenerateToken(It.IsAny<YetAnotherJira.Domain.User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidUsername_ShouldThrowUnauthorizedAccessException()
    {
        var handler = new LoginCommandHandler(DbContext, MockJwtService.Object, GetLogger<LoginCommandHandler>());
        var command = new LoginCommand("nonexistent", "password");
        
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        var handler = new LoginCommandHandler(DbContext, MockJwtService.Object, GetLogger<LoginCommandHandler>());
        var command = new LoginCommand("admin", "wrongpassword");
        
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = await DbContext.Users.FirstAsync(u => u.Username == "admin");
        user.IsActive = false;
        await DbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(DbContext, MockJwtService.Object, GetLogger<LoginCommandHandler>());
        var command = new LoginCommand("admin", "admin123");
        
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("Invalid username or password");
    }

    [Theory]
    [InlineData("admin", "admin123")]
    [InlineData("user1", "user123")]
    public async Task Handle_DifferentValidUsers_ShouldReturnCorrectUser(string username, string password)
    {
        var handler = new LoginCommandHandler(DbContext, MockJwtService.Object, GetLogger<LoginCommandHandler>());
        var command = new LoginCommand(username, password);
        
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.User.Username.Should().Be(username);
        result.Token.Should().Be("test-jwt-token");
    }
}
