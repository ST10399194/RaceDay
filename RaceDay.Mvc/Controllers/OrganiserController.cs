using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
