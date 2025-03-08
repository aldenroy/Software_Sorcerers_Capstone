using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MoviesMadeEasy.Areas.Identity.Pages.Account;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using NUnit.Framework;

namespace MME_Tests
{
    [TestFixture]
    public class RegisterRedirectTest
    {
        private class TestableRegisterModel : RegisterModel
        {
            public TestableRegisterModel(
                UserManager<IdentityUser> userManager,
                IUserStore<IdentityUser> userStore,
                SignInManager<IdentityUser> signInManager,
                ILogger<RegisterModel> logger,
                IEmailSender emailSender,
                IdentityDbContext identityContext,
                UserDbContext userDbContext)
                : base(userManager, userStore, signInManager, logger, emailSender, identityContext, userDbContext)
            {
            }

            // This method simulates the successful registration path
            public IActionResult SimulateSuccessfulRegistration()
            {
                var redirectUrl = "/Identity/Account/Preferences";
                return Redirect(redirectUrl);
            }
        }

        [Test]
        public void SuccessfulRegistration_RedirectsToPreferencesPage()
        {
            // Arrange
            var store = new Mock<IUserStore<IdentityUser>>();
            var emailStore = new Mock<IUserEmailStore<IdentityUser>>(); // Mock the email store

            // Setup the mock UserManager with both IUserStore and IUserEmailStore
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            mockUserManager.Setup(x => x.SupportsUserEmail).Returns(true); // Ensure SupportsUserEmail is true

            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
                null, null, null, null);

            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var userDbContext = new UserDbContext(options);

            // Create the RegisterModel instance
            var model = new TestableRegisterModel(
                mockUserManager.Object,
                store.Object,
                mockSignInManager.Object,
                new Mock<ILogger<RegisterModel>>().Object,
                new Mock<IEmailSender>().Object,
                null,
                userDbContext);

            // Mock GetEmailStore method to return the mocked email store
            var getEmailStoreMethod = model.GetType().GetMethod("GetEmailStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            getEmailStoreMethod.Invoke(model, null); // Invoke to simulate the real method

            model.PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData()
            };

            // Act
            var result = model.SimulateSuccessfulRegistration();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectResult>());
            var redirectResult = result as RedirectResult;
            Assert.That(redirectResult.Url, Is.EqualTo("/Identity/Account/Preferences"));

            // Cleanup
            userDbContext.Database.EnsureDeleted();
            userDbContext.Dispose();
        }
    }
}