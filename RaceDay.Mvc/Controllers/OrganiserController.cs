using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController : Controller
{
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
}