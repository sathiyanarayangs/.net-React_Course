using DataAccess.Core.Auth;
using DataAccess.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DataAccess.Api.Controllers;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Username, string Role);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserStore _userStore;

    public AuthController(IUserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// Task 4.5 - identical login logic to Week 3's in-memory version;
    /// only FindByUsernameAsync's implementation changed, from a
    /// Dictionary lookup to a parameterized SQL query.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userStore.FindByUsernameAsync(request.Username);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(new LoginResponse(user.Username, user.Role));
    }
}
