using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Mappers;
using YetAnotherJira.Application.Services;
using YetAnotherJira.Domain;

namespace YetAnotherJira.Application.Commands;

public record LoginCommand(string Username, string Password) : IRequest<(User User, string Token)>;

[UsedImplicitly]
internal sealed class LoginCommandHandler(ITicketDbContext dbContext, IJwtService jwtService, ILogger<LoginCommandHandler> logger) 
    : IRequestHandler<LoginCommand, (User User, string Token)>
{
    public async Task<(User User, string Token)> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Login attempt for username: {Username}", request.Username);
        
        var userDal = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);

        if (userDal == null)
        {
            logger.LogWarning("Login failed: User {Username} not found or inactive", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, userDal.PasswordHash))
        {
            logger.LogWarning("Login failed: Invalid password for user {Username}", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        var user = UserMapper.Map(userDal);
        var token = jwtService.GenerateToken(user);

        logger.LogInformation("Successful login for user {Username} (ID: {UserId})", request.Username, user.Id);
        return (user, token);
    }
}
