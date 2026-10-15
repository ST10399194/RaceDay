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
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrollmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // === 1. POST /api/enrolments ===
    [HttpPost("api/enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrolmentRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return BadRequest("Participant profile context missing.");

        var eventExists = await _context.Events.AnyAsync(e => e.EventID == request.EventId);
        var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == request.CategoryId);
        if (!eventExists || !categoryExists) return NotFound("Target Event or Category not found.");

        var newEnrollment = new Enrollment
        {
            ParticipantID = participant.ParticipantID,
            EventID = request.EventId,
            RegistrationDate = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Enrollments.Add(newEnrollment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Successfully enrolled in the event!", EnrollmentId = newEnrollment.EnrollmentID });
    }

    // === 2. GET /api/enrolments/mine ===
    [HttpGet("api/enrolments/mine")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.UserID == userId);
        if (participant == null) return BadRequest("Participant context missing.");

        var enrollments = await _context.Enrollments
          .Where(e => e.ParticipantID == participant.ParticipantID)
          .Include(e => e.Event)
          .Select(e => new EnrolmentDto
          {
              Id = e.EnrollmentID,
              EventId = e.EventID,
              EventName = e.Event != null ? e.Event.EventName : "Unknown Event",
              Status = e.Status
          }).ToListAsync();

        return Ok(enrollments);
    }

    // === 3. GET /api/enrolments/event/{eventId} ===
    [HttpGet("api/enrolments/event/{eventId}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEnrollmentsByEvent(int eventId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return Forbid();

        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (raceEvent == null) return NotFound("Event not found.");
        if (raceEvent.OrganiserID != organiser.OrganiserID) return Forbid("You do not own this event.");

        var enrollments = await _context.Enrollments
          .Where(e => e.EventID == eventId)
          .Include(e => e.Participant)
          .ThenInclude(p => p!.User)
          .Select(e => new EnrolmentDto
          {
              Id = e.EnrollmentID,
              EventId = e.EventID,
              EventName = raceEvent.EventName,
              UserEmail = e.Participant != null && e.Participant.User != null ? e.Participant.User.Email : "Unknown",
              Status = e.Status
          }).ToListAsync();

        return Ok(enrollments);
    }

    // === NEW: 4. PUT /api/enrolments/{id}/status ===
    [HttpPut("api/enrolments/{id}/status")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEnrollmentStatus(int id, [FromBody] UpdateEnrolmentStatusRequest request)
    {
        // 1. Grab logged-in Organiser account details
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return Forbid();

        // 2. Locate the target enrollment registration record
        var enrollment = await _context.Enrollments
      .Include(e => e.Event)
      .FirstOrDefaultAsync(e => e.EnrollmentID == id);

        if (enrollment == null) return NotFound("Enrollment record not found.");

        // 3. Validation rule check: Ensure this Organiser owns the event connected to this enrollment
        if (enrollment.Event == null || enrollment.Event.OrganiserID != organiser.OrganiserID)
        {
            return Forbid("Access denied. Only the correct Organiser can manage enrolments for their own Events.");
        }

        // Validate incoming string status parameter
        if (request.Status != "Pending" && request.Status != "Confirmed" && request.Status != "Cancelled")
        {
            return BadRequest("Status must be either 'Pending', 'Confirmed', or 'Cancelled'.");
        }

        // 4. Update and commit changes to the SQL Server
        enrollment.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Enrollment status successfully updated to '{request.Status}'." });
    }
}