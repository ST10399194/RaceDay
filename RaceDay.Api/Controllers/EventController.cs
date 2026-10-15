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
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // 1. GET /api/events -> Both roles may view public events list
    [HttpGet]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _context.Events.Select(e => new EventDto
        {
            Id = e.EventID,
            Name = e.EventName,
            Description = e.Description,
            Date = e.EventDate
        }).ToListAsync();

        return Ok(events);
    }

    // 2. GET /api/events/{id} -> Both roles may fetch specific race details
    [HttpGet("{id}")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (raceEvent == null) return NotFound("Requested race event could not be found.");

        var response = new EventDto
        {
            Id = raceEvent.EventID,
            Name = raceEvent.EventName,
            Description = raceEvent.Description,
            Date = raceEvent.EventDate
        };

        return Ok(response);
    }

    // 3. GET /api/events/mine -> Restricted to Organisers viewing their created inventory
    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetMyEvents()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return BadRequest("Active user is not properly configured as an Organiser.");

        var myEvents = await _context.Events
          .Where(e => e.OrganiserID == organiser.OrganiserID)
          .Select(e => new EventDto
          {
              Id = e.EventID,
              Name = e.EventName,
              Description = e.Description,
              Date = e.EventDate
          }).ToListAsync();

        return Ok(myEvents);
    }

    // 4. POST /api/events -> Organiser only
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null) return BadRequest("Active user profile validation failed.");

        // Default or placeholder configuration mapping for distance/category architecture mapping constraints
        var newEvent = new Event
        {
            EventName = request.Name,
            Description = request.Description,
            EventDate = request.Date,
            OrganiserID = organiser.OrganiserID,
            Location = "Default Location Value Required By Models",
            CategoryID = 1 // Default category reference assignment configuration to maintain table validation constraints
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Race Event successfully scheduled!", EventId = newEvent.EventID });
    }

    // 5. PUT /api/events/{id} -> Organisers can update events ONLY if they own them
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (raceEvent == null) return NotFound("Race event not found.");

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null || raceEvent.OrganiserID != organiser.OrganiserID)
        {
            return Forbid("Access denied. You cannot modify a race event that you did not create.");
        }

        raceEvent.EventName = request.Name;
        raceEvent.Description = request.Description;
        raceEvent.EventDate = request.Date;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Race event parameters updated successfully." });
    }

    // 6. DELETE /api/events/{id} -> Organisers can delete events ONLY if they own them
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (raceEvent == null) return NotFound("Race event not found.");

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        if (organiser == null || raceEvent.OrganiserID != organiser.OrganiserID)
        {
            return Forbid("Access denied. You cannot delete a race event that you did not create.");
        }

        _context.Events.Remove(raceEvent);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Race event deleted successfully." });
    }
}