
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using System.Net;

namespace RaceDay.Mvc.Controllers;

public class EventsController : Controller
{
    private readonly RaceDayApiClient _apiClient;

    public EventsController(RaceDayApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Gets an event from the API and displays its details.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // Validate the event ID.
        if (id <= 0)
            return BadRequest("Invalid event ID.");

        // Request the event from the API.
        using var response = await _apiClient.SendAsync(
            HttpMethod.Get,
            $"api/events/{id}"
        );

        // Check whether the event exists.
        if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound("Event not found.");

        // Handle API errors.
        if (!response.IsSuccessStatusCode)
            return StatusCode(
                (int)response.StatusCode,
                "Unable to retrieve event details from the API."
            );

        // Read the API response.
        var eventDetails =
            await _apiClient.ReadAsync<Dtos.EventDto>(response);

        if (eventDetails == null)
            return NotFound("Event details could not be loaded.");

        // Pass the event to the Razor view.
        return View("Details", eventDetails);
    }
}
