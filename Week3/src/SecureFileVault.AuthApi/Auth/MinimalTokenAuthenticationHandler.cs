using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SecureFileVault.Core.Auth;

namespace SecureFileVault.AuthApi.Auth;

/// <summary>
/// Task 3.15 - plugs MinimalJwt into ASP.NET Core's built-in authentication
/// pipeline, so the standard [Authorize(Roles = "Teacher")] attribute works
/// unmodified. This uses only Microsoft.AspNetCore.Authentication.Abstractions
/// (part of the ASP.NET Core shared framework via Microsoft.NET.Sdk.Web) —
/// no external JWT-bearer package — since the token itself is intentionally
/// minimal and hand-rolled (see MinimalJwt.cs).
/// </summary>
public class MinimalTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "MinimalToken";

    private readonly string _jwtSecret;

    public MinimalTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthApiSecrets secrets)
        : base(options, logger, encoder)
    {
        _jwtSecret = secrets.JwtSecret;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.NoResult()); // no header at all: anonymous, not a failure

        string headerValue = authHeader.ToString();
        const string prefix = "Bearer ";
        if (!headerValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Authorization header is not a Bearer token."));

        string token = headerValue[prefix.Length..].Trim();

        if (!MinimalJwt.TryValidate(token, _jwtSecret, out var payload) || payload is null)
            return Task.FromResult(AuthenticateResult.Fail("Token is invalid, tampered, or expired."));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, payload.Subject),
            new Claim(ClaimTypes.Role, payload.Role),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Tiny DI-friendly holder for the JWT signing secret, so it's configured
/// once in Program.cs and shared by both the login endpoint (which issues
/// tokens) and this handler (which validates them) without a static field.
/// </summary>
public class AuthApiSecrets
{
    public required string JwtSecret { get; init; }
}
