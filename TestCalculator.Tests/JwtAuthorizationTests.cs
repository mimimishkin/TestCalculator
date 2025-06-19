using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestCalculator.WebApi;
using TestCalculator.Domain;
using Microsoft.Data.Sqlite;

namespace TestCalculator.Tests;

public class JwtAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly SqliteConnection _connection;

    public JwtAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<OperationLogDbContext>>();
                services.AddDbContext<OperationLogDbContext>(options => options.UseSqlite(_connection));
            });
        });

        // Ensure the schema is created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OperationLogDbContext>();
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<string> RegisterAndLoginAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var registerContent = new StringContent(JsonSerializer.Serialize(new { Username = username, Password = password }), Encoding.UTF8, "application/json");
        var registerResp = await client.PostAsync("/api/token/register", registerContent);
        if (registerResp.StatusCode != HttpStatusCode.OK && registerResp.StatusCode != HttpStatusCode.Conflict)
            throw new Exception("Registration failed");
        var loginContent = new StringContent(JsonSerializer.Serialize(new { Username = username, Password = password }), Encoding.UTF8, "application/json");
        var loginResp = await client.PostAsync("/api/token/login", loginContent);
        var loginBody = await loginResp.Content.ReadAsStringAsync();
        if (loginResp.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Login failed: {loginBody}");
        var json = JsonDocument.Parse(loginBody);
        return json.RootElement.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/calculator/add?a=1&b=2");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid token");
        var resp = await client.GetAsync("/api/calculator/add?a=1&b=2");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync("user1", "pass1");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = await client.GetAsync("/api/calculator/add?a=1&b=2");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("result", body);
    }

    [Fact]
    public async Task Register_ExistingUser_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(new { Username = "user2", Password = "pass2" }), Encoding.UTF8, "application/json");
        var resp1 = await client.PostAsync("/api/token/register", content);
        Assert.Equal(HttpStatusCode.OK, resp1.StatusCode);
        var resp2 = await client.PostAsync("/api/token/register", content);
        Assert.Equal(HttpStatusCode.Conflict, resp2.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(new { Username = "user3", Password = "pass3" }), Encoding.UTF8, "application/json");
        await client.PostAsync("/api/token/register", content);
        var wrongLogin = new StringContent(JsonSerializer.Serialize(new { Username = "user3", Password = "wrong" }), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/api/token/login", wrongLogin);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(new { Username = "no_user", Password = "no_pass" }), Encoding.UTF8, "application/json");
        var resp = await client.PostAsync("/api/token/login", content);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_ReturnsUnauthorized()
    {
        // Manually create an expired token
        var jwt = CreateExpiredJwt("user4");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        var resp = await client.GetAsync("/api/calculator/add?a=1&b=2");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    private string CreateExpiredJwt(string username)
    {
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes("Also try Terraria! Do not try LEGO Fortnite!"));
        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "TestCalculator",
            audience: "TestCalculatorUsers",
            claims: [new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, username)],
            expires: DateTime.UtcNow.AddMinutes(-5), // expired
            signingCredentials: creds
        );
        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
} 