using Microsoft.AspNetCore.Mvc;
using SecureFileVault.AuthApi.Auth;
using SecureFileVault.Core.Auth;

namespace SecureFileVault.AuthApi.Controllers;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Role);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserStore _userStore;
    private readonly AuthApiSecrets _secrets;

    public AuthController(UserStore userStore, AuthApiSecrets secrets)
    {
        _userStore = userStore;
        _secrets = secrets;
    }

    /// <summary>
    /// Task 3.15 - verifies the PBKDF2 password hash and, on success, issues
    /// a minimal JWT carrying the user's role. Deliberately returns the same
    /// generic 401 whether the username doesn't exist or the password is
    /// wrong — distinguishing the two would tell an attacker which usernames
    /// are valid.
    /// </summary>
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var user = _userStore.FindByUsername(request.Username);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password." });

        string token = MinimalJwt.Issue(user.Username, user.Role, TimeSpan.FromHours(1), _secrets.JwtSecret);
        return Ok(new LoginResponse(token, user.Role));
    }
}
