using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RegistrationForm.Model;

namespace RegistrationForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> userManager;

        public HomeController(Microsoft.AspNetCore.Identity.UserManager<User> userManager)
        {
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
           
        var user = await userManager.GetUserAsync(User);
            if (user != null) {
                var model = new UserViewModel
                {
                    Name = user.Name
                };
                return View(model);
            }
            return View();
        }
    }
}
