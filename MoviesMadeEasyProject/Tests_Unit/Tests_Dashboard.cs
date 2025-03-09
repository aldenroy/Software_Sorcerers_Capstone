using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.DTOs;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using MoviesMadeEasy.Controllers;

namespace MME_Tests
{
    // Subscription Repository Tests
    [TestFixture]
    public class StreamingServiceAndDashboardTests
    {
        private Mock<UserDbContext> _mockContext;
        private SubscriptionRepository _repository;
        private List<StreamingService> _streamingServices;
        private List<UserStreamingService> _userStreamingServices;

        [SetUp]
        public void Setup()
        {
            var mockOptions = new DbContextOptionsBuilder<UserDbContext>().Options;
            _mockContext = new Mock<UserDbContext>(mockOptions);

            _streamingServices = new List<StreamingService>
            {
                new StreamingService
                {
                    Id = 1, Name = "Netflix", Region = "US", BaseUrl = "https://www.netflix.com/login",
                    LogoUrl = "/images/Netflix_Symbol_RGB.png", UserStreamingServices = new List<UserStreamingService>()
                },
                new StreamingService
                {
                    Id = 2, Name = "Hulu", Region = "US", BaseUrl = "https://auth.hulu.com/web/login",
                    LogoUrl = "/images/hulu-Green-digital.png", UserStreamingServices = new List<UserStreamingService>()
                },
                new StreamingService
                {
                    Id = 3, Name = "Disney+", Region = "US", BaseUrl = "https://www.disneyplus.com/login",
                    LogoUrl = "/images/disney_logo_march_2024_050fef2e.png",
                    UserStreamingServices = new List<UserStreamingService>()
                },
                new StreamingService
                {
                    Id = 4, Name = "Amazon Prime Video", Region = "US", BaseUrl = "https://www.amazon.com/ap/signin",
                    LogoUrl = "/images/AmazonPrimeVideo.png", UserStreamingServices = new List<UserStreamingService>()
                }
            };

            var users = new List<User>
            {
                new User { Id = 1, FirstName = "Test" }
            }.AsQueryable();

            var mockUsersDbSet = MockHelper.GetMockDbSet(users);
            _mockContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            _userStreamingServices = new List<UserStreamingService>();

            var mockStreamingServicesDbSet = MockHelper.GetMockDbSet(_streamingServices.AsQueryable());
            var mockUserStreamingServicesDbSet = MockHelper.GetMockDbSet(_userStreamingServices.AsQueryable());

            _mockContext.Setup(c => c.StreamingServices).Returns(mockStreamingServicesDbSet.Object);
            _mockContext.Setup(c => c.UserStreamingServices).Returns(mockUserStreamingServicesDbSet.Object);

            _repository = new SubscriptionRepository(_mockContext.Object);
        }

        [Test]
        public void GetAllServices_ReturnsAllServices()
        {
            var result = _repository.GetAllServices().ToList();

            Assert.AreEqual(4, result.Count, "Expected all 4 services to be returned.");
            Assert.IsTrue(result.Any(s => s.Name == "Netflix"));
            Assert.IsTrue(result.Any(s => s.Name == "Hulu"));
            Assert.IsTrue(result.Any(s => s.Name == "Disney+"));
            Assert.IsTrue(result.Any(s => s.Name == "Amazon Prime Video"));
        }


        [Test]
        public void GetAllServices_ReturnsListInAlphabeticalOrder()
        {
            var result = _repository.GetAllServices().ToList();

            var expectedOrder = result.OrderBy(s => s.Name).ToList();

            Assert.IsTrue(result.SequenceEqual(expectedOrder),
                "The list of streaming services is not in alphabetical order.");
        }

        [Test]
        public void AddUserSubscriptions_NullOrEmptyList_DoesNotThrowException()
        {
            int userId = 1;
            Assert.DoesNotThrow(() => _repository.AddUserSubscriptions(userId, new List<int>()));
            Assert.DoesNotThrow(() => _repository.AddUserSubscriptions(userId, null));
        }


        [Test]
        public void AddUserSubscriptions_NonExistentUser_ThrowsException()
        {
            int userId = 99;
            var ex = Assert.Throws<InvalidOperationException>(() => _repository.AddUserSubscriptions(userId, new List<int> { 1 }));
            Assert.That(ex.Message, Is.EqualTo("User does not exist."));
        }

        [Test]
        public void AddUserSubscriptions_AlreadySubscribedService_DoesNotAddDuplicate()
        {
            int userId = 1;
            _userStreamingServices.Add(new UserStreamingService { UserId = userId, StreamingServiceId = 1 });
            _repository.AddUserSubscriptions(userId, new List<int> { 1 });
            Assert.AreEqual(1, _userStreamingServices.Count(us => us.UserId == userId && us.StreamingServiceId == 1));
        }
    }

    // User Controller Tests
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ISubscriptionRepository> _subscriptionServiceMock;
        private MoviesMadeEasy.Controllers.UserController _controller;
        private DashboardDTO _dashboard;

        [SetUp]
        public void Setup()
        {
            var dummyLogger = Mock.Of<ILogger<UserController>>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _subscriptionServiceMock = new Mock<ISubscriptionRepository>();
            _controller = new MoviesMadeEasy.Controllers.UserController(dummyLogger, null, _userRepositoryMock.Object, _subscriptionServiceMock.Object);
            _dashboard = new DashboardDTO { UserName = "Test" };
            var httpContext = new DefaultHttpContext();
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }


        [Test]
        public void SaveSubscriptions_WithEmptySelectedServices_RedirectsToSubscriptionForm()
        {
            int userId = 1;
            string selectedServices = "";
            var result = _controller.SaveSubscriptions(userId, selectedServices) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("SubscriptionForm", result.ActionName);
            Assert.AreEqual(userId, result.RouteValues["userId"]);
        }

        [Test]
        public void SaveSubscriptions_WithValidSelectedServices_ReturnsDashboardView()
        {
            int userId = 1;
            string selectedServices = "1,2";
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" },
                new StreamingService { Name = "Hulu" }
            };
            var user = new User { Id = userId, FirstName = "TestUser" };

            _subscriptionServiceMock.Setup(s => s.AddUserSubscriptions(userId, It.IsAny<List<int>>()));
            _subscriptionServiceMock.Setup(s => s.GetUserSubscriptions(userId)).Returns(subscriptions);
            _userRepositoryMock.Setup(r => r.GetUser(userId)).Returns(user);

            var result = _controller.SaveSubscriptions(userId, selectedServices) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Dashboard", result.ViewName);
            var model = result.Model as DashboardDTO;
            Assert.IsNotNull(model);
            Assert.AreEqual(userId, model.UserId);
            Assert.AreEqual(user.FirstName, model.UserName);
            Assert.IsTrue(model.HasSubscriptions);
            Assert.AreEqual(2, model.SubList.Count);
        }

        [Test]
        public void SaveSubscriptions_UserNotFound_ReturnsDashboardViewWithEmptyUserName()
        {
            int userId = 1;
            string selectedServices = "1";
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" }
            };

            _subscriptionServiceMock.Setup(s => s.AddUserSubscriptions(userId, It.IsAny<List<int>>()));
            _subscriptionServiceMock.Setup(s => s.GetUserSubscriptions(userId)).Returns(subscriptions);
            _userRepositoryMock.Setup(r => r.GetUser(userId)).Returns((User)null);

            var result = _controller.SaveSubscriptions(userId, selectedServices) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Dashboard", result.ViewName);
            var model = result.Model as DashboardDTO;
            Assert.IsNotNull(model);
            Assert.AreEqual(userId, model.UserId);
            Assert.AreEqual("", model.UserName);
        }

        [Test]
        public void Dashboard_ZeroSubscriptions_ShowsEmptyList()
        {
            _dashboard.HasSubscriptions = false;
            _dashboard.SubList = new List<StreamingService>();
            Assert.That(_dashboard.HasSubscriptions, Is.False);
            Assert.That(_dashboard.SubList, Is.Empty);
            Assert.That(_dashboard.SubList.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dashboard_OneSubscription_ShowsSingleService()
        {
            _dashboard.HasSubscriptions = true;
            _dashboard.SubList = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" }
            };
            Assert.That(_dashboard.HasSubscriptions, Is.True);
            Assert.That(_dashboard.SubList, Is.Not.Empty);
            Assert.That(_dashboard.SubList.Count, Is.EqualTo(1));
            Assert.That(_dashboard.SubList.First().Name, Is.EqualTo("Netflix"));
        }

        [Test]
        public void Dashboard_MultipleSubscriptions_ShowsAllServices()
        {
            _dashboard.HasSubscriptions = true;
            _dashboard.SubList = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" },
                new StreamingService { Name = "Hulu" },
                new StreamingService { Name = "Disney+" }
            };
            Assert.That(_dashboard.HasSubscriptions, Is.True);
            Assert.That(_dashboard.SubList.Count, Is.EqualTo(3));
            Assert.That(_dashboard.SubList.Select(s => s.Name).Contains("Netflix"));
            Assert.That(_dashboard.SubList.Select(s => s.Name).Contains("Hulu"));
            Assert.That(_dashboard.SubList.Select(s => s.Name).Contains("Disney+"));
        }

        [Test]
        public void SaveSubscriptions_WhenExceptionThrown_DisplaysErrorMessageAndReturnsSubscriptionForm()
        {
            int userId = 1;
            string selectedServices = "1,2";

            _subscriptionServiceMock
                .Setup(s => s.AddUserSubscriptions(userId, It.IsAny<List<int>>()))
                .Throws(new Exception("Test exception"));
            _userRepositoryMock.Setup(r => r.GetUser(userId)).Returns(new User { Id = userId, FirstName = "TestUser" });

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var result = _controller.SaveSubscriptions(userId, selectedServices) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("SubscriptionForm", result.ViewName);
            Assert.IsTrue(_controller.TempData.ContainsKey("Message"));
            Assert.AreEqual("There was an issue managing your subscription. Please try again later.", _controller.TempData["Message"]);
        }


        [Test]
        public void SaveSubscriptions_Success_ShouldSetTempDataMessageAndReturnDashboardView()
        {
            int userId = 1;
            string selectedServices = "1,2,3";
            var user = new User { Id = userId, FirstName = "TestUser" };

            _userRepositoryMock.Setup(repo => repo.GetUser(userId)).Returns(user);
            _subscriptionServiceMock.Setup(service => service.GetUserSubscriptions(userId))
                .Returns(new List<StreamingService>());
            _subscriptionServiceMock.Setup(service => service.AddUserSubscriptions(userId, It.IsAny<List<int>>()));


            var result = _controller.SaveSubscriptions(userId, selectedServices);


            Assert.AreEqual("Subscriptions managed successfully!", _controller.TempData["Message"]);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "Expected a ViewResult.");
            Assert.AreEqual("Dashboard", viewResult.ViewName);
        }

    }
}
