using MediatR;
using Microsoft.AspNetCore.Mvc;
using YetAnotherJira.Application.Commands;
using YetAnotherJira.Models.V1.Requests;
using YetAnotherJira.Models.V1.Responses;

namespace YetAnotherJira.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<V1LoginResponse>> Login([FromBody] V1LoginRequest request, CancellationToken token)
    {
        try
        {
            var (user, jwtToken) = await mediator.Send(new LoginCommand(request.Username, request.Password), token);

            return Ok(new V1LoginResponse
            {
                Token = jwtToken,
                Username = user.Username,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) 
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken()
    {
        return Ok(new { message = "Token is valid", user = User.Identity.Name });
    }
}
