using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Entities;

namespace YetAnotherJira.Application.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
    string GetUsernameFromToken(string token);
}
