using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoviesMadeEasy.Controllers;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.DTOs;
using MoviesMadeEasy.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;

namespace MME_Tests
{
    [TestFixture]
    public class DashboardTests
    {
        private Mock<IMovieService> _mockMovieService;
        private Mock<ISubscriptionService> _mockSubscriptionService;
        private HomeController _controller;
        private Mock<UserManager<User>> _mockUserManager;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _mockMovieService = new Mock<IMovieService>();
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _controller = new HomeController(_mockMovieService.Object, _mockSubscriptionService.Object, _mockUserManager.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testUser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var mockUser = new User
            {
                UserName = "testUser",
                Email = "testUser@example.com"
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);
        }

        [TearDown]
        public void Teardown()
        {
            _controller.Dispose();
        }

        [Test]
        public void Dashboard_WhenNoUserSubscriptions_ShouldReturnDtoWithNoSubscriptions()
        {
            // Arrange
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns(new List<StreamingService>());

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsFalse(model.HasSubscriptions);
            Assert.IsEmpty(model.SubscriptionNames);
            Assert.IsEmpty(model.SubscriptionLogos);
        }

        [Test]
        public void Dashboard_WhenUserHasSingleSubscription_ShouldReturnDtoWithOneSubscription()
        {
            // Arrange
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix", LogoUrl = "netflix-logo.png" }
            };
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns(subscriptions);

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsTrue(model.HasSubscriptions);
            Assert.That(model.SubscriptionNames.Count, Is.EqualTo(1));
            Assert.That(model.SubscriptionNames[0], Is.EqualTo("Netflix"));
            Assert.That(model.SubscriptionLogos[0], Is.EqualTo("netflix-logo.png"));
        }

        [Test]
        public void Dashboard_WhenUserHasMultipleSubscriptions_ShouldReturnDtoWithMultipleSubscriptions()
        {
            // Arrange
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix", LogoUrl = "netflix-logo.png" },
                new StreamingService { Name = "Hulu", LogoUrl = "hulu-logo.png" }
            };
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns(subscriptions);

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsTrue(model.HasSubscriptions);
            Assert.That(model.SubscriptionNames.Count, Is.EqualTo(2));
            Assert.Contains("Netflix", model.SubscriptionNames);
            Assert.Contains("Hulu", model.SubscriptionNames);
            Assert.Contains("netflix-logo.png", model.SubscriptionLogos);
            Assert.Contains("hulu-logo.png", model.SubscriptionLogos);
        }

        [Test]
        public void Dashboard_WhenSubscriptionServiceReturnsNull_ShouldReturnDtoWithNoSubscriptions()
        {
            // Arrange
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns((List<StreamingService>?)null);

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsFalse(model.HasSubscriptions);
            Assert.IsEmpty(model.SubscriptionNames);
            Assert.IsEmpty(model.SubscriptionLogos);
        }

        [Test]
        public void Dashboard_WhenSubscriptionNamesAreEmpty_ShouldHandleGracefully()
        {
            // Arrange
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "", LogoUrl = "netflix-logo.png" }
            };
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns(subscriptions);

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsTrue(model.HasSubscriptions);
            Assert.That(model.SubscriptionNames.Count, Is.EqualTo(1));
            Assert.That(model.SubscriptionNames[0], Is.EqualTo(""));
            Assert.That(model.SubscriptionLogos[0], Is.EqualTo("netflix-logo.png"));
        }

        [Test]
        public void Dashboard_WhenSubscriptionLogoUrlIsNull_ShouldHandleGracefully()
        {
            // Arrange
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix", LogoUrl = null }
            };
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns(subscriptions);

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsTrue(model.HasSubscriptions);
            Assert.That(model.SubscriptionNames.Count, Is.EqualTo(1));
            Assert.That(model.SubscriptionNames[0], Is.EqualTo("Netflix"));
            Assert.IsNull(model.SubscriptionLogos[0]);
        }

        [Test]
        public void Dashboard_WhenMixedValidAndInvalidSubscriptions_ShouldHandleGracefully()
        {
            // Arrange
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix", LogoUrl = "netflix-logo.png" },
                new StreamingService { Name = "", LogoUrl = null }
            };
            _mockSubscriptionService.Setup(s => s.GetUserSubscriptions(It.IsAny<string>()))
                                    .Returns(subscriptions);

            // Act
            var result = _controller.Dashboard().Result as ViewResult;
            Assert.NotNull(result);
            var model = result?.Model as DashboardDTO;

            // Assert
            Assert.NotNull(model);
            Assert.IsTrue(model.HasSubscriptions);
            Assert.That(model.SubscriptionNames.Count, Is.EqualTo(2));
            Assert.Contains("Netflix", model.SubscriptionNames);
            Assert.Contains("", model.SubscriptionNames);
            Assert.Contains("netflix-logo.png", model.SubscriptionLogos);
            Assert.Contains(null, model.SubscriptionLogos);
        }
    }
}
