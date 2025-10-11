using BlogData.Services;
using BlogData.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IDataService _dataService;
    private readonly IConfiguration _configuration;

    public AuthController(IDataService dataService, IConfiguration configuration)
    {
        _dataService = dataService;
        _configuration = configuration;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Message = "Invalid model" });

        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest(new { Message = "Username cannot be null or empty" });

        if (request.UserName.Length < 2)
            return BadRequest(new { Message = "Username must be at least 2 characters long" });

        if (request.UserName.Length > 50)
            return BadRequest(new { Message = "Username cannot exceed 50 characters" });

        if (!request.UserName.All(char.IsLetterOrDigit))
            return BadRequest(new { Message = "Username can only contain letters and digits" });

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { Message = "Password cannot be null or empty" });

        if (request.Password.Length < 3)
            return BadRequest(new { Message = "Password must be at least 3 characters long" });

        // Check if user already exists
        var userExists = await _dataService.UserExistsAsync(request.UserName);
        if (userExists)
        {
            return BadRequest(new { Message = "Username already exists" });
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.UserName + "@test.com",
            PasswordHash = request.Password // In real app, hash this password
        };

        var createdUser = await _dataService.CreateUserAsync(user);
        var token = GenerateJwtToken(createdUser);

        return Ok(new
        {
            Token = token,
            User = new
            {
                createdUser.Id,
                createdUser.Email,
                createdUser.UserName
            }
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _dataService.GetUserByNameAsync(request.UserName);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        var isValidPassword = await _dataService.ValidateUserPasswordAsync(request.UserName, request.Password);
        if (!isValidPassword)
        {
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.Email,
                user.UserName
            }
        });
    }

    // POST: api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        return Ok(new { Message = "Logged out successfully" });
    }

    // GET: api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _dataService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong";
        var issuer = jwtSettings["Issuer"] ?? "BlogApi";
        var audience = jwtSettings["Audience"] ?? "BlogApp";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
