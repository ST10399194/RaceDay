using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api)
    {
        _api = api;
    }

    private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserId").HasValue;

    private bool IsOrganiser() =>
        string.Equals(HttpContext.Session.GetString("Role"), "Organiser",
            StringComparison.OrdinalIgnoreCase);

    public IActionResult Index()
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Auth");

        if (!IsOrganiser())
            return RedirectToAction("AccessDenied", "Home");

        return View();
    }

    public async Task<IActionResult> Events()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events/mine");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();
        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventRequest model)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(model);

        var response = await _api.SendAsync(HttpMethod.Post, "api/events", model);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(model);
        }

        return RedirectToAction(nameof(Events));
    }

    public async Task<IActionResult> Categories(int eventId)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
        var categories = await _api.ReadAsync<List<CategoryDto>>(response) ?? new();

        return View(categories);
    }
}
