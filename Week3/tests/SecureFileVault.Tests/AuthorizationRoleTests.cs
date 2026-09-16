using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SecureFileVault.AuthApi.Controllers;
using Xunit;

namespace SecureFileVault.Tests;

/// <summary>
/// Task 3.15's specific proof point: "prove a Student token gets 403 and a
/// Teacher token gets 200." This needs the real HTTP pipeline (authentication
/// + authorization middleware actually running), so it uses
/// WebApplicationFactory rather than calling the controller directly.
/// </summary>
public class AuthorizationRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthorizationRoleTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200AndToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("alice.teacher", "TeacherPass123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal("Teacher", body.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("alice.teacher", "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostGrade_WithTeacherToken_Returns200()
    {
        string token = await LoginAsync("alice.teacher", "TeacherPass123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/grades", new PostGradeRequest("Bob", "A"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostGrade_WithStudentToken_Returns403()
    {
        string token = await LoginAsync("bob.student", "StudentPass123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/grades", new PostGradeRequest("Bob", "A"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostGrade_WithNoToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/grades", new PostGradeRequest("Bob", "A"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGrades_WithStudentToken_Returns200_NoRoleRestriction()
    {
        string token = await LoginAsync("bob.student", "StudentPass123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/grades");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
