
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.Mvc.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
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

    // Check whether the user is logged in as a Participant.
    private bool IsParticipant()
    {
        return HttpContext.Session.GetInt32("UserId").HasValue &&
               string.Equals(
                   HttpContext.Session.GetString("Role"),
                   "Participant",
                   StringComparison.OrdinalIgnoreCase);
    }

    // Display upcoming events.
    public async Task<IActionResult> Index(string? search)
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/events");

        if (!response.IsSuccessStatusCode)
            return View(new List<EventDto>());

        var events = await _api.ReadAsync<List<EventDto>>(response)
                     ?? new List<EventDto>();

        // Show only upcoming events.
        events = events
            .Where(e => e.Date >= DateTime.Today)
            .ToList();

        // Search by event name.
        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events
                .Where(e => e.Name.Contains(
                    search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return View(events);
    }

    // Enter an event.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Auth");

        var request = new CreateEnrolmentRequest
        {
            EventId = eventId,
            CategoryId = categoryId
        };

        var response = await _api.SendAsync(
            HttpMethod.Post, "api/enrolments", request);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] =
                await response.Content.ReadAsStringAsync();

            return RedirectToAction("Details", "Events",
                new { id = eventId });
        }

        TempData["Success"] =
            "You have entered the event successfully.";

        return RedirectToAction(nameof(MyEvents));
    }

    // Display the Participant's enrolled events.
    public async Task<IActionResult> MyEvents()
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/enrolments/mine");

        if (!response.IsSuccessStatusCode)
            return View(new List<EnrolmentDto>());

        var enrolments =
            await _api.ReadAsync<List<EnrolmentDto>>(response)
            ?? new List<EnrolmentDto>();

        return View(enrolments);
    }

    // Display the Participant's results.
    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/results/mine");

        if (!response.IsSuccessStatusCode)
            return View(new List<ResultDto>());

        var results =
            await _api.ReadAsync<List<ResultDto>>(response)
            ?? new List<ResultDto>();

        return View(results);
    }

    // Display the Participant's profile.
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(
            HttpMethod.Get, "api/profile/me");

        if (!response.IsSuccessStatusCode)
            return NotFound("Profile could not be retrieved.");

        var profile =
            await _api.ReadAsync<ProfileDto>(response);

        if (profile == null)
            return NotFound("Profile not found.");

        return View(profile);
    }

    // Update the Participant's profile.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        ProfileDto model,
        IFormFile? profilePicture)
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        // Upload a profile picture if one was selected.
        if (profilePicture != null && profilePicture.Length > 0)
        {
            using var content = new MultipartFormDataContent();
            using var stream = profilePicture.OpenReadStream();
            using var streamContent = new StreamContent(stream);

            content.Add(
                streamContent,
                "file",
                Path.GetFileName(profilePicture.FileName));

            var imageResponse = await _api.SendAsync(
                HttpMethod.Post,
                "api/profile/upload-image",
                content);

            if (!imageResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Failed to upload profile picture.");

                return View(model);
            }
        }

        // Save profile information.
        var response = await _api.SendAsync(
            HttpMethod.Put, "api/profile/me", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "",
                await response.Content.ReadAsStringAsync());

            return View(model);
        }

        TempData["Success"] = "Profile updated successfully.";

        return RedirectToAction(nameof(Profile));
    }
}

