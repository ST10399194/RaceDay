
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Mvc.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiClient _api;

    public AuthController(RaceDayApiClient api)
    {
        _api = api;
    }

    // Display registration page.
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Register a new user.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var request = new RegisterRequest
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email,
            Password = vm.Password,
            Role = vm.Role
        };

        var response = await _api.SendAsync(
            HttpMethod.Post,
            "api/auth/register",
            request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "",
                await response.Content.ReadAsStringAsync());

            return View(vm);
        }

        TempData["Success"] = "Registration successful. Please log in.";

        return RedirectToAction(nameof(Login));
    }

    // Display login page.
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Authenticate an existing user.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var request = new LoginRequest
        {
            Email = vm.Email,
            Password = vm.Password
        };

        var response = await _api.SendAsync(
            HttpMethod.Post,
            "api/auth/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }

        var auth = await _api.ReadAsync<AuthResponse>(response);

        if (auth == null ||
            string.IsNullOrWhiteSpace(auth.Role) ||
            auth.UserId <= 0)
        {
            ModelState.AddModelError("", "Invalid login response.");
            return View(vm);
        }

        // Save the API authentication cookie if one is returned.
        var apiCookie = RaceDayApiClient.ExtractSessionCookie(response);

        if (!string.IsNullOrWhiteSpace(apiCookie))
        {
            HttpContext.Session.SetString("ApiSessionCookie", apiCookie);
        }

        // Save user information in the MVC session.
        HttpContext.Session.SetInt32("UserId", auth.UserId);
        HttpContext.Session.SetString("Role", auth.Role);

        HttpContext.Session.SetString(
            "DisplayName",
            string.IsNullOrWhiteSpace(auth.FirstName)
                ? auth.Email
                : auth.FirstName);

        // Redirect according to the user's role.
        if (string.Equals(auth.Role, "Organiser",
            StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Organiser");
        }

        if (string.Equals(auth.Role, "Participant",
            StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Participant");
        }

        HttpContext.Session.Clear();

        ModelState.AddModelError("", "Unsupported account role.");

        return View(vm);
    }

    // Log out the user.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _api.SendAsync(
                HttpMethod.Post,
                "api/auth/logout");
        }
        finally
        {
            HttpContext.Session.Clear();
        }

        return RedirectToAction("Index", "Home");
    }
}

