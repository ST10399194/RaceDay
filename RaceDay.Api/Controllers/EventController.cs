
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
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

    // Converts an Event database record into an EventDto.
    private static EventDto ToDto(Event e)
    {
        return new EventDto
        {
            Id = e.EventID,
            Name = e.EventName,
            Description = e.Description,
            Date = e.EventDate,
            Location = e.Location,
            BannerImageUrl = e.BannerImageUrl
        };
    }

    // Gets the logged-in user's ID from the JWT.
    private bool TryGetUserId(out int userId)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }

    // GET: api/events
    // Organisers and Participants can view all events.
    [HttpGet]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _context.Events
            .Select(e => new EventDto
            {
                Id = e.EventID,
                Name = e.EventName,
                Description = e.Description,
                Date = e.EventDate,
                Location = e.Location,
                BannerImageUrl = e.BannerImageUrl
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/events/{id}
    // Gets one event.
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        return Ok(ToDto(raceEvent));
    }

    // GET: api/events/mine
    // Organisers can only view their own events.
    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetMyEvents()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        var myEvents = await _context.Events
            .Where(e => e.OrganiserID == organiser.OrganiserID)
            .Select(e => new EventDto
            {
                Id = e.EventID,
                Name = e.EventName,
                Description = e.Description,
                Date = e.EventDate,
                Location = e.Location,
                BannerImageUrl = e.BannerImageUrl
            })
            .ToListAsync();

        return Ok(myEvents);
    }

    // POST: api/events
    // Only Organisers can create events.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        if (request == null)
            return BadRequest("Event details are required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Event name is required.");

        if (request.Name.Length > 100)
            return BadRequest("Event name cannot exceed 100 characters.");

        if (request.Date == default || request.Date <= DateTime.UtcNow)
            return BadRequest("A valid future event date is required.");

        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        var newEvent = new Event
        {
            EventName = request.Name.Trim(),
            Description = request.Description,
            EventDate = request.Date,
            Location = string.IsNullOrWhiteSpace(request.Location)
                ? "Location to be confirmed"
                : request.Location.Trim(),
            OrganiserID = organiser.OrganiserID,

            // Keep this only if CategoryID is required by your Event model.
            CategoryID = 1
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEventById),
            new { id = newEvent.EventID },
            ToDto(newEvent));
    }

    // PUT: api/events/{id}
    // Organisers can update only their own events.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request)
    {
        if (request == null)
            return BadRequest("Event details are required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Event name is required.");

        if (request.Name.Length > 100)
            return BadRequest("Event name cannot exceed 100 characters.");

        if (request.Date == default || request.Date <= DateTime.UtcNow)
            return BadRequest("A valid future event date is required.");

        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserID != organiser.OrganiserID)
            return Forbid();

        raceEvent.EventName = request.Name.Trim();
        raceEvent.Description = request.Description;
        raceEvent.EventDate = request.Date;

        await _context.SaveChangesAsync();

        return Ok(ToDto(raceEvent));
    }

    // DELETE: api/events/{id}
    // Organisers can delete only their own events.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        if (raceEvent.OrganiserID != organiser.OrganiserID)
            return Forbid();

        _context.Events.Remove(raceEvent);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Event successfully deleted." });
    }

    // POST: api/events/{id}/banner
    // Only the event's Organiser can upload a banner.
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("{id:int}/banner")]
    [Authorize(Roles = "Organiser")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBanner(
        int id,
        [FromForm] IFormFile file,
        [FromServices] BlobStorageService blobStorage)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please select a banner image.");

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        if (eventItem.OrganiserID != organiser.OrganiserID)
            return Forbid();

        try
        {
            var imageUrl = await blobStorage.UploadAsync(file, "banners");

            eventItem.BannerImageUrl = imageUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Banner uploaded successfully.",
                Url = imageUrl
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

}

