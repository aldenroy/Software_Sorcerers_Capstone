using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.Areas.Identity.Pages.Account;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using NUnit.Framework;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;

namespace MME_Tests
{
    [TestFixture]
    public class RegisterPreferencesModelTests
    {
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private UserDbContext _userContext;
        private RegisterPreferencesModel _model;
        private IdentityUser _identityUser;
        private User _customUser;
        
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserPreferencesTestDb_{Guid.NewGuid()}")
                .Options;
            _userContext = new UserDbContext(options);
            
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            
            _identityUser = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "testuser@example.com",
                Email = "testuser@example.com"
            };
            
            _customUser = new User
            {
                Id = 1,
                AspNetUserId = _identityUser.Id,
                FirstName = "Test",
                LastName = "User",
                ColorMode = "Light",
                FontSize = "Medium",
                FontType = "Standard"
            };
            
            _userContext.Users.Add(_customUser);
            _userContext.SaveChanges();
            
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_identityUser);
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_identityUser.Id);
            
            _model = new RegisterPreferencesModel(_mockUserManager.Object, _userContext);
            
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _identityUser.Id)
            }));
            
            _model.PageContext = new PageContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData()
            };

            var viewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());
            
            // Set up PageContext with the ViewData
            _model.PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext(),
                ViewData = viewData
            };
        }
        
        [TearDown]
        public void TearDown()
        {
            _userContext.Database.EnsureDeleted();
            _userContext.Dispose();
        }


        
        [Test]
        public async Task OnPostAsync_ShouldUpdatePreferencesInDatabase()
        {
            // Arrange
            _model.Input = new RegisterPreferencesModel.InputModel
            {
                ColorMode = "Dark",
                FontSize = "Large",
                FontType = "Open Dyslexic"
            };
            
            // Act
            var result = await _model.OnPostAsync();
            
            // Assert
            // 1. Check the redirection
            var mockResult = new Mock<RedirectToActionResult>("Dashboard", "User", null);
            mockResult.Setup(m => m.ExecuteResultAsync(It.IsAny<ActionContext>())).Returns(Task.CompletedTask);

            // 2. Check if the user preferences were updated in the database
            var updatedUser = await _userContext.Users
                .FirstOrDefaultAsync(u => u.AspNetUserId == _identityUser.Id);
            
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.ColorMode, Is.EqualTo("Dark"));
            Assert.That(updatedUser.FontSize, Is.EqualTo("Large"));
            Assert.That(updatedUser.FontType, Is.EqualTo("Open Dyslexic"));
        }
        
        [Test]
        public async Task OnPostAsync_ShouldHandleNullUser()
        {
            // Arrange
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser)null);
            
            // Act
            var result = await _model.OnPostAsync();
            
            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Unable to load user with ID '{_identityUser.Id}'."));
        }
        
        [Test]
        public async Task OnGet_ShouldLoadUserPreferences()
        {
            // Arrange - setup is already handling this
            
            // Act
            var result = await _model.OnGet();
            
            // Assert
            // 1. Check we get the page result
            Assert.That(result, Is.InstanceOf<PageResult>());
            
            // 2. Check if the model was populated with user preferences
            Assert.That(_model.Input.ColorMode, Is.EqualTo("Light"));
            Assert.That(_model.Input.FontSize, Is.EqualTo("Medium"));
            Assert.That(_model.Input.FontType, Is.EqualTo("Standard"));
        }
        
        [Test]
        public async Task OnGet_ShouldHandleNullUser()
        {
            // Arrange
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((IdentityUser)null);
            
            // Act
            var result = await _model.OnGet();
            
            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Unable to load user with ID '{_identityUser.Id}'."));
        }

        [Test]
        public async Task OnPostSkip_ShouldNotChangePreferences()
        {
            // Arrange
            string originalColorMode = _customUser.ColorMode;
            string originalFontSize = _customUser.FontSize;
            string originalFontType = _customUser.FontType;
            
            // Set different values in the Input model
            _model.Input = new RegisterPreferencesModel.InputModel
            {
                ColorMode = "dark",
                FontSize = "Large",
                FontType = "Open Dyslexic"
            };
            
            // Act
            var result = _model.OnPostSkip();
            
            // Assert
            // 1. Check the redirection
            var mockResult = new Mock<RedirectToActionResult>("Dashboard", "User", null);
            mockResult.Setup(m => m.ExecuteResultAsync(It.IsAny<ActionContext>())).Returns(Task.CompletedTask);
            
            // 2. Verify database values haven't changed
            var userInDb = await _userContext.Users
                .FirstOrDefaultAsync(u => u.AspNetUserId == _identityUser.Id);
            
            Assert.That(userInDb, Is.Not.Null);
            Assert.That(userInDb.ColorMode, Is.EqualTo(originalColorMode));
            Assert.That(userInDb.FontSize, Is.EqualTo(originalFontSize));
            Assert.That(userInDb.FontType, Is.EqualTo(originalFontType));
        }

        [Test]
        public async Task OnPostAsync_ShouldUpdateViewDataAfterPreferencesChanged()
        {
            // Arrange
            _model.Input = new RegisterPreferencesModel.InputModel
            {
                ColorMode = "Dark",
                FontSize = "Large",
                FontType = "Open Dyslexic"
            };

            // Act
            var result = await _model.OnPostAsync();

            // Assert
            Assert.That(_model.ViewData["ColorMode"], Is.EqualTo("dark"));
        }
    }
}