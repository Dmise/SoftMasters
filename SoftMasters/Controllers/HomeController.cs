using Microsoft.AspNetCore.Mvc;

namespace SoftMasters.test.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
