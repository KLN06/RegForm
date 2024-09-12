using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RegistrationForm.Controllers;
using RegistrationForm.Model;
using RegistrationForm.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace RegistrationForm.Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<Microsoft.AspNetCore.Identity.UserManager<User>> userManagerMock;
        private Mock<SignInManager<User>> signInManagerMock;
        private UserController userController;

        [SetUp]
        public void Setup()
        {
            userManagerMock = new Mock<Microsoft.AspNetCore.Identity.UserManager<User>>(
                Mock.Of<Microsoft.AspNetCore.Identity.IUserStore<User>>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            signInManagerMock = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null,
                null,
                null,
                null);

            userController = new UserController(userManagerMock.Object, signInManagerMock.Object, null);
        }
        [TearDown] public void TearDown()
        {
            userController.Dispose();
        }

        [Test]
        public void Login_Get_ReturnsViewResult()
        {
            var result = userController.Login() as ViewResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Login_Post_ValidModel_RedirectsToHome()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "password"
            };

            var user = new User { Email = model.Email };
            userManagerMock.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(user.UserName, model.Password, false, false))
                             .ReturnsAsync(SignInResult.Success);

            var result = await userController.Login(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_InvalidModel_ReturnsView()
        {
            userController.ModelState.AddModelError("error", "error");
            var model = new LoginViewModel { Email = "invalid", Password = "invalid" };

            var result = await userController.Login(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Login_Post_InvalidPassword_ThrowsException()
        {
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "invalidpassword"
            };

            var user = new User { UserName = "TestUser", Email = model.Email };

            userManagerMock.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(user.UserName, model.Password, false, false))
                             .ReturnsAsync(SignInResult.Failed);

            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await userController.Login(model));
            Assert.That(ex.Message, Is.EqualTo("Invalid login attempt"));
        }


        [Test]
        public void Register_Get_ReturnsViewResult()
        {
            var result = userController.Register() as ViewResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Register_Post_ValidModel_RedirectsToHome()
        {
            var model = new RegisterViewModel
            {
                Name = "Test",
                Email = "test@example.com",
                Password = "#Password123",
                ConfirmPassword = "#Password123",
                IsoCode="+359",
                Telephone="889534450"
            };

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                IsoCode = model.IsoCode,
                Telephone = model.Telephone
            };

            userManagerMock.Setup(um => um.CreateAsync(It.Is<User>(u => u.Email == model.Email), model.Password))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);


            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(user.UserName, model.Password, false, false))
                             .ReturnsAsync(SignInResult.Success);

            var result = await userController.Register(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Register_Post_InvalidModel_ReturnsView()
        {
            userController.ModelState.AddModelError("error", "error");
            var model = new RegisterViewModel();

            var result = await userController.Register(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Register_Post_InvalidModelState_ReturnsViewWithModel()
        {
            var model = new RegisterViewModel
            {
                Name = "TestUser",
                Email = "testuser@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123",
                IsoCode = "+359",
                Telephone = "0889534450"
            };

            userController.ModelState.AddModelError("Email", "Email is required");

            var result = await userController.Register(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(result.Model, Is.EqualTo(model));
        }

        [Test]
        public async Task Register_Post_DuplicateEmail_ReturnsViewWithErrors()
        {
            var model = new RegisterViewModel
            {
                Name = "TestUser",
                Email = "testuser@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123",
                IsoCode = "+359",
                Telephone = "0889534450"
            };

            var existingUser = new User
            {
                Email = model.Email,
                UserName = model.Email
            };

            userManagerMock.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync(existingUser);

            signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<User>(), It.IsAny<bool>(), null))
                .Returns(Task.CompletedTask);

            var result = await userController.Register(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(result.Model, Is.EqualTo(model));

            Assert.That(userController.ModelState["Email"].Errors.Count, Is.EqualTo(1));
            Assert.That(userController.ModelState["Email"].Errors[0].ErrorMessage, Is.EqualTo("This email is already registered."));
        }



        [Test]
        public async Task Edit_Post_ValidModel_UpdatesUserAndReturnsHome()
        {
            var model = new EditViewModel()
            {
                Name = "UpdatedName",
                CurrentPassword = "currentPassword123",
                NewPassword = "newPassword123",
                ConfirmPassword = "newPassword123"
            };

            var user = new User
            {
                UserName = "TestUser",
                Name = "TestName"
            };

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            userManagerMock.Setup(um => um.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);

            userManagerMock.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);

            signInManagerMock.Setup(sm => sm.RefreshSignInAsync(user))
                .Returns(Task.CompletedTask);

            var result = await userController.Edit(model) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Edit_Post_UserNull_ThrowsException()
        {
            var model = new EditViewModel()
            {
                Name = "UpdatedName",
                CurrentPassword = "currentPassword123",
                NewPassword = "newPassword123",
                ConfirmPassword = "newPassword123"
            };

            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User) null);

            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await userController.Edit(model));

            Assert.That(exception.Message, Is.EqualTo("User not found. Make sure you are registered/logged in."));
        }

        [Test]
        public async Task Logout_Post_SignsOutAndReturnsHome()
        {
            signInManagerMock.Setup(sm => sm.SignOutAsync())
                .Returns (Task.CompletedTask);

            var result = await userController.Logout() as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

    }
}

