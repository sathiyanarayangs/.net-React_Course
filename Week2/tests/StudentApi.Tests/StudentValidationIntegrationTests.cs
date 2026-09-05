using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using StudentApi.Dtos;
using Xunit;

namespace StudentApi.Tests;

/// <summary>
/// Task 2.6's proof point specifically needs the real ASP.NET Core model
/// binding/validation pipeline — that only runs over an actual HTTP request,
/// not a direct call to a controller method — so this one test class uses
/// WebApplicationFactory instead of a plain unit test.
/// </summary>
public class StudentValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StudentValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_WithMissingRequiredFields_Returns400_Automatically()
    {
        // Empty body: fails [Required] on Name/Email and [Range] on Age/Score.
        var invalidDto = new StudentCreateDto { Name = "", Age = 0, Email = "", Score = -5 };

        var response = await _client.PostAsJsonAsync("/api/students", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidEmail_Returns400()
    {
        var invalidDto = new StudentCreateDto { Name = "Priya", Age = 20, Email = "not-an-email", Score = 80 };

        var response = await _client.PostAsJsonAsync("/api/students", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAgeOutsideRange_Returns400()
    {
        var invalidDto = new StudentCreateDto { Name = "Priya", Age = 101, Email = "priya@example.com", Score = 80 };

        var response = await _client.PostAsJsonAsync("/api/students", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidBody_Returns201()
    {
        var validDto = new StudentCreateDto { Name = "Priya", Age = 20, Email = "priya@example.com", Score = 80 };

        var response = await _client.PostAsJsonAsync("/api/students", validDto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
