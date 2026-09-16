using Microsoft.AspNetCore.Authentication;
using SecureFileVault.AuthApi.Auth;
using SecureFileVault.Core.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SecureFileVault Auth API",
        Version = "v1",
        Description = "Week 3, Task 3.15 - minimal PBKDF2 + hand-rolled JWT login with a server-side role check."
    });

    // Lets Swagger's "Try it out" send a Bearer token.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste just the token — Swagger adds the 'Bearer ' prefix."
    });
});

// Task 3.15 - in-memory user store (PBKDF2-hashed passwords, seeded with one
// Teacher and one Student account) and the shared JWT signing secret.
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton(new AuthApiSecrets
{
    // A real deployment would pull this from configuration/secret storage,
    // never hardcode it. It's fixed here only because this is a minimal,
    // deliberately small teaching slice (see the Week 3 brief).
    JwtSecret = builder.Configuration["Jwt:Secret"] ?? "demo-only-secret-do-not-use-in-real-systems"
});

builder.Services
    .AddAuthentication(MinimalTokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, MinimalTokenAuthenticationHandler>(
        MinimalTokenAuthenticationHandler.SchemeName, options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "SecureFileVault Auth API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication(); // must run before UseAuthorization — establishes WHO the caller is
app.UseAuthorization();  // then decides WHAT they may do, using the roles UseAuthentication attached
app.MapControllers();

app.Run();

public partial class Program { }
