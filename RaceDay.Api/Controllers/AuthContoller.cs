using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
using System.Security.Cryptography;
using System.Text;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public AuthController(RaceDayDbContext context)
    {
        _context = context;
    }

    // === REGISTRATION FLOW ===
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. Validate request and check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("A user with this email already exists.");
        }

        // Validate Role input
        if (request.Role != "Participant" && request.Role != "Organiser")
        {
            return BadRequest("Role must be either 'Participant' or 'Organiser'.");
        }

        // 2. Securely hash the password (never store raw text strings!)
        string hashedPassword = HashPassword(request.Password);

        // 3. Save User details to SQL Server
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

        // Automatically create the dependent profile structure based on the chosen role
        if (request.Role == "Participant")
        {
            _context.Participants.Add(new Participant { UserID = newUser.UserID });
        }
        else if (request.Role == "Organiser")
        {
            _context.Organisers.Add(new Organiser { UserID = newUser.UserID });
        }
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Registration successful!" });
    }

    // === LOGIN & SESSION MANAGEMENT FLOW ===
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Fetch user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 2. Hash incoming password and compare to saved hash
        string incomingHash = HashPassword(request.Password);
        if (user.PasswordHash != incomingHash)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 3. Return a simulated token response tracking the session profile info
        // (Tracks UserId and Role required by Step 14 layout)
        var response = new AuthResponse
        {
            Email = user.Email,
            Token = $"SIMULATED_JWT_TOKEN_FOR_USER_{user.UserID}_ROLE_{user.Role}"
        };

        return Ok(response);
    }

    // === PASSWORD HASHING HELPER ===
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}