using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MoviesMadeEasy.Controllers;
using MoviesMadeEasy.Models;
using NUnit.Framework;
using System.Threading.Tasks;

namespace MME_Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<ILogger<UserController>> _mockLogger;
        private Mock<SignInManager<IdentityUser>> _mockSignInManager;
        private UserController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                new Mock<UserManager<IdentityUser>>(new Mock<IUserStore<IdentityUser>>().Object, null, null, null, null, null, null, null, null).Object,
                null, // IHttpContextAccessor (Not needed)
                null, // IUserClaimsPrincipalFactory (Not needed)
                null, null, null, null
            );

            _controller = new UserController(_mockLogger.Object, _mockSignInManager.Object);
        }


        [TearDown]
        public void Teardown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Login_Successful_ShouldRedirectToDashboard()
        {
            // Arrange
            string email = "test@example.com";
            string password = "Password123";

            _mockSignInManager
                .Setup(s => s.PasswordSignInAsync(email, password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success); // Fix ambiguity

            // Act
            var result = await _controller.Login(email, password, "/Dashboard") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Dashboard", result.ActionName);
        }
    }
}
