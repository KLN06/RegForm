using Microsoft.AspNetCore.Mvc;

namespace RegistrationForm.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
