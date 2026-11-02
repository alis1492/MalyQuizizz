using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using quizApi.Models;
using quizApi.Options;
using quizApi.Services;
using quizShared.DTOs;

namespace quizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserService userService,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthController> logger
) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
    {
        var existing = await _userService.GetByEmailAsync(dto.Email);
        if (existing is not null)
            return Conflict(new { message = "Email already registered." });

        var user = await _userService.CreateAsync(dto);

        _logger.LogInformation("User registered: {UserId} ({Email})", user.Id, user.Email);
        return CreatedAtAction(nameof(Register), ToDto(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var user = await _userService.GetByEmailAsync(dto.Email);

        // Same generic message for "no such user" and "wrong password" —
        // don't tell an attacker which emails exist.
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = GenerateToken(user);
        _logger.LogInformation("User logged in: {UserId}", user.Id);

        return Ok(new
        {
            token,
            user = ToDto(user)
        });
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.UserType.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDTO ToDto(User user) => new()
    {
        Id = user.Id.ToString(),
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        UserType = user.UserType,
        CreatedAtUtc = user.CreatedAtUtc
    };
}