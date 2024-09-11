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

namespace RegistrationForm.Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<UserManager<User>> userManagerMock;
        private Mock<SignInManager<User>> signInManagerMock;
        private UserController userController;

        [SetUp]
        public void Setup()
        {
            userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
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

        [Test]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = userController.Login() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Login_Post_ValidModel_RedirectsToHome()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "password"
            };

            var user = new User { UserName = model.Email };
            userManagerMock.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            signInManagerMock.Setup(sm => sm.PasswordSignInAsync(user.UserName, model.Password, false, false))
                             .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await userController.Login(model) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            userController.ModelState.AddModelError("error", "error");
            var model = new LoginViewModel { Email = "invalid", Password = "invalid" };

            // Act
            var result = await userController.Login(model) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = userController.Register() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Register_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            userController.ModelState.AddModelError("error", "error");
            var model = new RegisterViewModel();

            // Act
            var result = await userController.Register(model) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
        }


    }
}

