﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;
using RegistrationForm.Data;
using RegistrationForm.Helpers;
using RegistrationForm.Model;
using System.Net;
using System.Net.Mail;
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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
                    else
                    {
                        throw new ArgumentException("Invalid login attempt");
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "Email not registered yet.");
                }
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult Register()
        {
            ModelState.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string CaptchaInput)
        {

            string sessionCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (CaptchaInput != sessionCaptcha)
            {
                return View(model);
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                IsoCode = model.IsoCode,
                Telephone = model.Telephone,
                Gender = model.Gender,
                Address = model.Address
            };

            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model); 
            }

            var result = await userManager.CreateAsync(user, model.Password);

            if(result != null)
            {
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }

            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var model = new EditViewModel
            {
                Name = user.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ArgumentException("User not found. Make sure you are registered/logged in.");
            }

            if(model.Name != null)
            {
                user.Name = model.Name;
                user.UserName = model.Name;
            }

            if (model.NewPassword != null && model.ConfirmPassword != null) 
            {
                var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.ConfirmPassword);
                    if (!result.Succeeded)
                    {
                        return View(model);
                    }
                
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return View(model);
            }
            else
            {
                await signInManager.RefreshSignInAsync(user); // refresh the autentication cookie so that the changes i have made are applied.
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult GetCaptcha()
        {
            string captchaText = CaptchaHelper.GenerateCaptchaText(6);
            HttpContext.Session.SetString("CaptchaCode", captchaText);
            byte[] captchaImage = CaptchaHelper.GenerateCaptchaImage(captchaText);
            return File(captchaImage, "image/png");
        }
    }

}
