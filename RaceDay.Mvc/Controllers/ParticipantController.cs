using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api)
    {
        _api = api;
    }

    private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId").HasValue;

    private bool IsParticipant() =>
        string.Equals(HttpContext.Session.GetString("Role"), "Participant",
            StringComparison.OrdinalIgnoreCase);

    //  Upcoming events page with filtering
    public async Task<IActionResult> Index(string? type, string? search)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();

        events = events.Where(e => e.EventDate >= DateTime.Today).ToList();

        if (!string.IsNullOrWhiteSpace(type))
            events = events.Where(e => e.EventType == type).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e =>
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return View(events);
    }

    // Event details and category selection
    [HttpPost]
    public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var request = new CreateEnrolmentRequest
        {
            EventId = eventId,
            CategoryId = categoryId
        };

        var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments", request);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        TempData["Success"] = "You have entered the event successfully.";
        return RedirectToAction(nameof(MyEvents));
    }

    // My Events
    public async Task<IActionResult> MyEvents()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        // Calls the participant enrolment endpoint from Part 2 API
        var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
        var myEnrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new();

        return View(myEnrolments);
    }

    //  My Results
    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        // Queries the participant's official logged history from Part 2 API
        var response = await _api.SendAsync(HttpMethod.Get, "api/results/mine");
        var myResults = await _api.ReadAsync<List<ResultDto>>(response) ?? new();

        return View(myResults);
    }

    // Profile - Display Form
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/profile");
        var profile = await _api.ReadAsync<ParticipantProfileDto>(response);

        if (profile == null) return NotFound("Profile not found.");

        return View(profile);
    }

    // Profile - Handle Update & Image Upload to API
    [HttpPost]
    public async Task<IActionResult> Profile(ParticipantProfileDto model, IFormFile? profilePicture)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(model);

        // If an image is selected, handle uploading multi-part form content to Part 2 API
        if (profilePicture != null && profilePicture.Length > 0)
        {
            using var content = new MultipartFormDataContent();
            using var stream = profilePicture.OpenReadStream();
            using var streamContent = new StreamContent(stream);

            // Match the parameter expectation of  endpoint
            content.Add(streamContent, "file", profilePicture.FileName);

            var imgResponse = await _api.SendAsync(HttpMethod.Post, "api/profile/upload-image", content);
            if (!imgResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to save profile picture to cloud storage.");
                return View(model);
            }
        }

        // Persist standard profile field data shifts
        var response = await _api.SendAsync(HttpMethod.Put, "api/profile", model);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(model);
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

}
