using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers
{
    public class AuthContoller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
