using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")] // Mapped precisely to /api/auth blueprint path
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(RaceDayDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // === 1. POST /api/auth/register ===
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check whether the email address already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { Message = "A user with this email address already exists." });
        }

        // Confirm the selected role is valid
        if (request.Role != "Participant" && request.Role != "Organiser")
        {
            return BadRequest(new { Message = "Invalid role selection. Must be 'Participant' or 'Organiser'." });
        }

        // Hash the password securely
        string hashedPassword = HashPassword(request.Password);

        // Save the user
        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = hashedPassword,
            Role = request.Role
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Dynamically instantiate dependent profile structural components
        if (request.Role == "Participant")
        {
            _context.Participants.Add(new Participant { UserID = newUser.UserID });
        }
        else if (request.Role == "Organiser")
        {
            _context.Organisers.Add(new Organiser { UserID = newUser.UserID });
        }
        await _context.SaveChangesAsync();

        return Ok(new { Message = "User registration successfully completed." });
    }

    // === 2. POST /api/auth/login ===
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Find the user using the supplied email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid email credentials or password matching failure." });
        }

        // Verify the supplied password against the stored hash
        string incomingHash = HashPassword(request.Password);
        if (user.PasswordHash != incomingHash)
        {
            return Unauthorized(new { Message = "Invalid email credentials or password matching failure." });
        }

        // Create the session / Store UserId and Role inside token claims payload
        var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Role, user.Role)
    };

        var keySecret = _configuration["JwtSettings:Secret"] ?? "SuperSecretDefaultTestingKeyChangeThisInProduction123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new JwtSecurityToken(
          issuer: _configuration["JwtSettings:Issuer"] ?? "RaceDayApi",
          audience: _configuration["JwtSettings:Audience"] ?? "RaceDayApp",
          claims: claims,
          expires: DateTime.UtcNow.AddHours(4),
          signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        // Return useful authenticated-user information
        return Ok(new AuthResponse
        {
            Email = user.Email,
            Token = tokenString
        });
    }

    // === 3. POST /api/auth/logout ===
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Instructs client terminal environments to purge local storage session arrays
        return Ok(new { Message = "Session successfully cleared. Protected functionality is no longer accessible." });
    }

    // Cryptographic security hash generation helper method
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}