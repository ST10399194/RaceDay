using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;
using System.Security.Claims;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // === 1. POST /api/results  ===
    // Restricted to Organisers ONLY. Validates event ownership before saving.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult([FromBody] CreateResultRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized(); // 401 Unauthorized

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return Forbid(); // 403 Forbidden

        // Find the target enrollment record and include its event data connection
        var enrollment = await _context.Enrollments
      .Include(e => e.Event)
      .FirstOrDefaultAsync(e => e.EnrollmentID == request.EnrolmentId);

        if (enrollment == null) return NotFound("Target enrollment record not found."); // 404 Not Found
        if (enrollment.Event == null) return NotFound("Connected race event not found.");

        // Ownership Validation constraint rule check
        if (enrollment.Event.OrganiserID != organiser.OrganiserID)
        {
            return Forbid("Access denied. You cannot record results for a race event you do not own."); // 403 Forbidden
        }

        // Prevent duplicate scoring files entries
        var currentResultExists = await _context.Results.AnyAsync(r => r.EnrollmentID == request.EnrolmentId);
        if (currentResultExists) return BadRequest("A score entry has already been registered for this runner."); // 400 Bad Request

        var newResult = new Result
        {
            EnrollmentID = request.EnrolmentId,
            FinishTime = request.TimeTaken,
            Position = request.Position
        };

        _context.Results.Add(newResult);
        await _context.SaveChangesAsync();

        var responseDto = new ResultDto
        {
            Id = newResult.ResultID,
            EnrolmentId = newResult.EnrollmentID,
            TimeTaken = newResult.FinishTime,
            Position = newResult.Position
        };

        return StatusCode(201, responseDto); // 201 Created
    }

    // === 2. GET /api/results/mine  ===
    // Allows signed-in Participants to download their own scorecards
    [HttpGet("mine")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized(); // 401 Unauthorized

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return BadRequest("Participant profile context missing."); // 400 Bad Request

        var results = await _context.Results
      .Include(r => r.Enrollment)
      .ThenInclude(e => e!.Participant)
      .ThenInclude(p => p!.User)
      .Where(r => r.Enrollment != null && r.Enrollment.ParticipantID == participant.ParticipantID)
      .Select(r => new ResultDto
      {
          Id = r.ResultID,
          EnrolmentId = r.EnrollmentID,
          RunnerName = r.Enrollment!.Participant!.User != null
          ? $"{r.Enrollment.Participant.User.FirstName} {r.Enrollment.Participant.User.LastName}"
          : "Unknown Runner",
          TimeTaken = r.FinishTime,
          Position = r.Position
      }).ToListAsync();

        return Ok(results); // 200 OK
    }

    // === 3. GET /api/results/event/{eventId}  ===
    // Public leaderboard visible to both roles sorted by performance rankings
    [HttpGet("event/{eventId}")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetResultsByEvent(int eventId)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
        if (!eventExists) return NotFound("Target race event not found."); // 404 Not Found

        var results = await _context.Results
      .Include(r => r.Enrollment)
      .ThenInclude(e => e!.Participant)
      .ThenInclude(p => p!.User)
      .Where(r => r.Enrollment != null && r.Enrollment.EventID == eventId)
      .OrderBy(r => r.Position) // Automatically stack ranks leaderboard by 1st place downward
            .Select(r => new ResultDto
            {
                Id = r.ResultID,
                EnrolmentId = r.EnrollmentID,
                RunnerName = r.Enrollment!.Participant!.User != null
          ? $"{r.Enrollment.Participant.User.FirstName} {r.Enrollment.Participant.User.LastName}"
          : "Unknown Runner",
                TimeTaken = r.FinishTime,
                Position = r.Position
            }).ToListAsync();

        return Ok(results); // 200 OK
    }
}