using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TestCalculator.WebApi;

namespace TestCalculator.Tests;

public class JwtAuthorizationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsJwtToken()
    {
        var response = await _client.PostAsJsonAsync("/api/token/login", new { username = "testuser", password = "password" });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("token", out var tokenProp));
        Assert.False(string.IsNullOrWhiteSpace(tokenProp.GetString()));
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/calculator/add?a=1&b=2");
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_Returns200()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/token/login", new { username = "testuser", password = "password" });
        loginResponse.EnsureSuccessStatusCode();
        var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString();
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/calculator/add?a=1&b=2");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
} 