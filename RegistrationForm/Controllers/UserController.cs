﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistrationForm.Data;
using RegistrationForm.Model;
using System.Threading.Tasks;

namespace RegistrationForm.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext context;

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            context = dbContext;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Correctly await the FindByEmailAsync call to get the actual user
                var user = await userManager.FindByEmailAsync(model.Email);


                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user.Name, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // If the sign-in was successful, redirect to home or any other page
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
                    else
                    {
                        throw new ArgumentException("Credentials");
                    }
                }
                else
                {
                    throw new ArgumentException("Email");
                }
            }

            throw new ArgumentException("Invalid");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Log the ModelState errors or check in debugger
                return View(model);
            }

            var user = new User
            {
                UserName = model.Name,
                Email = model.Email,
                Name = model.Name,
                IsoCode = model.IsoCode,
                Telephone = model.Telephone,
                Gender = model.Gender,
                Address = model.Address
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

}
