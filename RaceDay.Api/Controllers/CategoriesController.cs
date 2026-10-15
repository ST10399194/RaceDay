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
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // === 1. GET /api/events/{eventId}/categories ===
    [HttpGet("api/events/{eventId}/categories")]
    [Authorize(Roles = "Organiser,Participant")]
    public async Task<IActionResult> GetCategoriesForEvent(int eventId)
    {
        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (raceEvent == null) return NotFound("Event not found.");

        // Return categories matching the event context
        var categories = await _context.Categories
      .Where(c => c.CategoryID == raceEvent.CategoryID)
      .Select(c => new CategoryDto
      {
          Id = c.CategoryID,
          Name = c.CategoryName
      }).ToListAsync();

        return Ok(categories);
    }

    // === 2. POST /api/events/{eventId}/categories ===
    [HttpPost("api/events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
    {
        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
        if (raceEvent == null) return NotFound("Event not found.");

        // Verify the active Organiser owns this event
        if (!await IsEventOwner(raceEvent.OrganiserID)) return Forbid("Access denied. You do not own this event.");

        var newCategory = new Category
        {
            CategoryName = request.Name,
            DistanceKM = 10.0 // Default or extract from additional payload details
        };

        _context.Categories.Add(newCategory);
        await _context.SaveChangesAsync();

        return Ok(new CategoryDto { Id = newCategory.CategoryID, Name = newCategory.CategoryName });
    }

    // === 3. PUT /api/categories/{id} ===
    [HttpPut("api/categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound("Category not found.");

        // Check ownership against an event mapped to this category
        var relatedEvent = await _context.Events.FirstOrDefaultAsync(e => e.CategoryID == id);
        if (relatedEvent != null && !await IsEventOwner(relatedEvent.OrganiserID))
        {
            return Forbid("Access denied. You do not own the event associated with this category.");
        }

        category.CategoryName = request.Name;
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Category updated successfully." });
    }

    // === 4. DELETE /api/categories/{id} ===
    [HttpDelete("api/categories/{id}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null) return NotFound("Category not found.");

        var relatedEvent = await _context.Events.FirstOrDefaultAsync(e => e.CategoryID == id);
        if (relatedEvent != null && !await IsEventOwner(relatedEvent.OrganiserID))
        {
            return Forbid("Access denied. You do not own the event associated with this category.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Category deleted successfully." });
    }

    private async Task<bool> IsEventOwner(int eventOrganiserId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId)) return false;

        var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserID == userId);
        return organiser != null && eventOrganiserId == organiser.OrganiserID;
    }
}