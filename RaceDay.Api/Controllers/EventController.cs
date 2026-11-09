

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

   
    // GET: api/events
    // Both Organisers and Participants can view events.
    
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
                Date = e.EventDate
            })
            .ToListAsync();

        return Ok(events);
    }

    
    // GET: api/events/{id}
    // View details of one event.
    
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        var response = new EventDto
        {
            Id = raceEvent.EventID,
            Name = raceEvent.EventName,
            Description = raceEvent.Description,
            Date = raceEvent.EventDate
        };

        return Ok(response);
    }

   
    // GET: api/events/mine
    // Organisers can view only their own events.

    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetMyEvents()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return BadRequest("Organiser profile not found.");

        var myEvents = await _context.Events
            .Where(e => e.OrganiserID == organiser.OrganiserID)
            .Select(e => new EventDto
            {
                Id = e.EventID,
                Name = e.EventName,
                Description = e.Description,
                Date = e.EventDate
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
        // Validate the event name.
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Event name is required.");

        if (request.Name.Length > 100)
            return BadRequest("Event name cannot exceed 100 characters.");

        // Validate the event date.
        if (request.Date == default)
            return BadRequest("A valid event date is required.");

        if (request.Date <= DateTime.UtcNow)
            return BadRequest("The event date must be in the future.");

        // Get the logged-in Organiser.
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return BadRequest("Organiser profile not found.");

        // Create the new event.
        var newEvent = new Event
        {
            EventName = request.Name.Trim(),
            Description = request.Description,
            EventDate = request.Date,
            OrganiserID = organiser.OrganiserID,

            // Temporary values from your original model.
            Location = "Default Location Value Required By Models",
            CategoryID = 1
        };

        _context.Events.Add(newEvent);

        await _context.SaveChangesAsync();

        // Return the created event.
        var response = new EventDto
        {
            Id = newEvent.EventID,
            Name = newEvent.EventName,
            Description = newEvent.Description,
            Date = newEvent.EventDate
        };

        return CreatedAtAction(
            nameof(GetEventById),
            new { id = newEvent.EventID },
            response);
    }

 
    // PUT: api/events/{id}
    // Organisers can update only their own events.
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request)
    {
        // Validate the request.
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Event name is required.");

        if (request.Name.Length > 100)
            return BadRequest("Event name cannot exceed 100 characters.");

        if (request.Date == default)
            return BadRequest("A valid event date is required.");

        if (request.Date <= DateTime.UtcNow)
            return BadRequest("The event date must be in the future.");

        // Identify the logged-in Organiser.
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        // Find the event.
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        // Check event ownership.
        if (raceEvent.OrganiserID != organiser.OrganiserID)
            return Forbid();

        // Update event information.
        raceEvent.EventName = request.Name.Trim();
        raceEvent.Description = request.Description;
        raceEvent.EventDate = request.Date;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Event successfully updated."
        });
    }

    
    // DELETE: api/events/{id}
    // Organisers can delete only their own events.
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        // Identify the logged-in Organiser.
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        // Find the event.
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (raceEvent == null)
            return NotFound("Event not found.");

        // Check event ownership.
        if (raceEvent.OrganiserID != organiser.OrganiserID)
            return Forbid();

        // Delete the event.
        _context.Events.Remove(raceEvent);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Event successfully deleted."
        });
    }

    
    // POST: api/events/{id}/banner
    // Only the event's Organiser can upload a banner.
  
    [HttpPost("{id:int}/banner")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UploadBanner(
        int id,
        [FromForm] IFormFile file,
        [FromServices] BlobStorageService blobStorage)
    {
        // Check that a file was selected.
        if (file == null || file.Length == 0)
            return BadRequest("Please select a banner image.");

        // Find the event.
        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.EventID == id);

        if (eventItem == null)
            return NotFound("Event not found.");

        // Identify the logged-in Organiser.
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserID == userId);

        if (organiser == null)
            return Forbid();

        // Ensure that the Organiser owns this event.
        if (eventItem.OrganiserID != organiser.OrganiserID)
            return Forbid();

        try
        {
            // Upload the banner to the banners directory.
            string imageUrl = await blobStorage.UploadAsync(
                file, "banners");

            // Store the uploaded image URL.
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
