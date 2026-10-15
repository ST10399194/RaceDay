using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Contracts;
using System.Security.Claims;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize] // Enforces secure session state across all profile tracking operations
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    // === GET /api/profile/me ===
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        // Extracts the logged-in UserID securely from the JWT token claims payload
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid session profile context.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
        {
            return NotFound("User profile matching the active session state not found.");
        }

        var response = new ProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        return Ok(response);
    }

    // === PUT /api/profile/me ===
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid session profile context.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
        {
            return NotFound("User profile matching the active session state not found.");
        }

        // Apply profile text updates
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Your profile information has been successfully updated." });
    }
}
