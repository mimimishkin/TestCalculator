using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using TestCalculator.Domain;

namespace TestCalculator.WebApi;

[ApiController]
[Route("api/[controller]")]
public class TokenController(IConfiguration configuration, AppDbContext dbContext) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] LoginRequest request)
    {
        if (dbContext.Users.Any(u => u.Username == request.Username))
            return Conflict("Username already exists");
        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Password)
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
        return Ok();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Username == request.Username);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash)) 
            return Unauthorized();
        
        var token = GenerateJwtToken(request.Username);
        return Ok(new { token });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!));
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8)
        );
        return $"{Convert.ToBase64String(salt)}.{hash}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8)
        );
        return hash == parts[1];
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 