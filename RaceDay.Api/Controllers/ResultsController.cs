
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

    // POST: api/results
    // Only Organisers can record results for their own events.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(
        [FromBody] CreateResultRequest request)
    {
        // Get the logged-in Organiser.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        // Find the enrolment and its associated event.
        var enrollment = await _context.Enrollments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrollmentID == request.EnrolmentId);

        if (enrollment == null)
            return NotFound("Enrolment record not found.");

        if (enrollment.Event == null)
            return NotFound("Associated event not found.");

        // Check that the Organiser owns the event.
        if (enrollment.Event.OrganiserID != organiser.OrganiserID)
            return Forbid();

        // Prevent duplicate results for the same enrolment.
        var resultExists = await _context.Results
            .AnyAsync(r => r.EnrollmentID == request.EnrolmentId);

        if (resultExists)
            return BadRequest("A result has already been recorded for this enrolment.");

        // Validate result information.
        if (string.IsNullOrWhiteSpace(request.TimeTaken))
            return BadRequest("Finish time is required.");

        if (request.Position <= 0)
            return BadRequest("Position must be greater than zero.");

        // Create the result.
        var newResult = new Result
        {
            EnrollmentID = request.EnrolmentId,
            FinishTime = request.TimeTaken,
            Position = request.Position
        };

        _context.Results.Add(newResult);
        await _context.SaveChangesAsync();

        // Return the created result.
        var response = new ResultDto
        {
            Id = newResult.ResultID,
            EnrolmentId = newResult.EnrollmentID,
            TimeTaken = newResult.FinishTime,
            Position = newResult.Position
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    // GET: api/results/mine
    // Participants can view their own results.
    [HttpGet("mine")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        // Get the logged-in Participant.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.UserID == userId);

        if (participant == null)
            return BadRequest("Participant profile not found.");

        // Retrieve only this Participant's results.
        var results = await _context.Results
            .Include(r => r.Enrollment)
            .ThenInclude(e => e!.Participant)
            .ThenInclude(p => p!.User)
            .Where(r => r.Enrollment != null &&
                        r.Enrollment.ParticipantID == participant.ParticipantID)
            .Select(r => new ResultDto
            {
                Id = r.ResultID,
                EnrolmentId = r.EnrollmentID,

                RunnerName = r.Enrollment!.Participant!.User != null
                    ? $"{r.Enrollment.Participant.User.FirstName} " +
                      $"{r.Enrollment.Participant.User.LastName}"
                    : "Unknown Runner",

                TimeTaken = r.FinishTime,
                Position = r.Position
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/results/event/{eventId}
    // Participants can view public leaderboards.
    // Organisers can only view leaderboards for their own events.
    [HttpGet("event/{eventId}")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetResultsByEvent(int eventId)
    {
        // Check whether the event exists.
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == eventId);

        if (raceEvent == null)
            return NotFound("Event not found.");

        // Apply ownership checks to Organisers.
        if (User.IsInRole("Organiser"))
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null)
                return Forbid();

            if (raceEvent.OrganiserID != organiser.OrganiserID)
                return Forbid();
        }

        // Retrieve the event leaderboard.
        var results = await _context.Results
            .Include(r => r.Enrollment)
            .ThenInclude(e => e!.Participant)
            .ThenInclude(p => p!.User)
            .Where(r => r.Enrollment != null &&
                        r.Enrollment.EventID == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new ResultDto
            {
                Id = r.ResultID,
                EnrolmentId = r.EnrollmentID,

                RunnerName = r.Enrollment!.Participant!.User != null
                    ? $"{r.Enrollment.Participant.User.FirstName} " +
                      $"{r.Enrollment.Participant.User.LastName}"
                    : "Unknown Runner",

                TimeTaken = r.FinishTime,
                Position = r.Position
            })
            .ToListAsync();

        return Ok(results);
    }
}
