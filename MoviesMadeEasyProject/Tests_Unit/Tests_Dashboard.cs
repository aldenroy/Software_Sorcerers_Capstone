using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using MoviesMadeEasy.Controllers;
using MoviesMadeEasy.Models.ModelView;
using MoviesMadeEasy.Models.DTO;

namespace MME_Tests
{
    // Subscription Repository Tests
    [TestFixture]
    public class SubscriptionRepositoryTests
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
                new StreamingService { Id = 1, Name = "Netflix" },
                new StreamingService { Id = 2, Name = "Hulu" },
                new StreamingService { Id = 3, Name = "Disney+" },
                new StreamingService { Id = 4, Name = "Amazon Prime Video" }
            };

            _userStreamingServices = new List<UserStreamingService>
            {
                new UserStreamingService { UserId = 1, StreamingServiceId = 1, MonthlyCost = 5.99m },
                new UserStreamingService { UserId = 1, StreamingServiceId = 2, MonthlyCost = 7.49m }
            };

            _userStreamingServices = new List<UserStreamingService>();

            var mockStreamingServicesDbSet = MockHelper.GetMockDbSet(_streamingServices.AsQueryable());
            var mockUserStreamingServicesDbSet = MockHelper.GetMockDbSet(_userStreamingServices.AsQueryable());

            mockUserStreamingServicesDbSet
                .Setup(x => x.AddRange(It.IsAny<IEnumerable<UserStreamingService>>()))
                .Callback<IEnumerable<UserStreamingService>>(items =>
                {
                    _userStreamingServices.AddRange(items);
                });

            mockUserStreamingServicesDbSet
                .Setup(x => x.RemoveRange(It.IsAny<IEnumerable<UserStreamingService>>()))
                .Callback<IEnumerable<UserStreamingService>>(items =>
                {
                    foreach (var item in items.ToList())
                    {
                        _userStreamingServices.Remove(item);
                    }
                });

            _mockContext.Setup(c => c.StreamingServices).Returns(mockStreamingServicesDbSet.Object);
            _mockContext.Setup(c => c.UserStreamingServices).Returns(mockUserStreamingServicesDbSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

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
        public void UpdateUserSubscriptions_EmptyDictionary_RemovesAllSubscriptions()
        {
            int userId = 1;

            _repository.UpdateUserSubscriptions(userId, new Dictionary<int, decimal>());

            Assert.IsEmpty(_userStreamingServices.Where(us => us.UserId == userId));
        }


        [Test]
        public void UpdateUserSubscriptions_AddsNewSubscriptions_WhenNoneExist()
        {
            int userId = 1;
            _userStreamingServices.Clear();

            var prices = new Dictionary<int, decimal>
            {
                { 1, 0m },
                { 3, 0m }
            };

            _repository.UpdateUserSubscriptions(userId, prices);

            var subscriptions = _userStreamingServices.Where(us => us.UserId == userId).ToList();
            Assert.AreEqual(2, subscriptions.Count);
            Assert.IsTrue(subscriptions.Any(us => us.StreamingServiceId == 1));
            Assert.IsTrue(subscriptions.Any(us => us.StreamingServiceId == 3));
            Assert.AreEqual(0m, subscriptions.Single(us => us.StreamingServiceId == 1).MonthlyCost);
            Assert.AreEqual(0m, subscriptions.Single(us => us.StreamingServiceId == 3).MonthlyCost);
        }

        [Test]
        public void UpdateUserSubscriptions_AlreadySubscribedService_DoesNotAddDuplicate()
        {
            int userId = 1;
            _userStreamingServices.Add(new UserStreamingService { UserId = userId, StreamingServiceId = 1 });

            var prices = new Dictionary<int, decimal>
            {
                { 1, 0m }
            };

            _repository.UpdateUserSubscriptions(userId, prices);

            var subscriptions = _userStreamingServices
                .Where(us => us.UserId == userId && us.StreamingServiceId == 1)
                .ToList();
            Assert.AreEqual(1, subscriptions.Count);
        }

        [Test]
        public void UpdateUserSubscriptions_RemovesUnselectedSubscriptions()
        {
            int userId = 1;
            _userStreamingServices.Add(new UserStreamingService { UserId = userId, StreamingServiceId = 1 });
            _userStreamingServices.Add(new UserStreamingService { UserId = userId, StreamingServiceId = 2 });

            var selectedIds = new[] { 2 };
            var prices = selectedIds.ToDictionary(id => id, id => 0m);

            _repository.UpdateUserSubscriptions(userId, prices);

            var subscriptions = _userStreamingServices.Where(us => us.UserId == userId).ToList();
            Assert.AreEqual(1, subscriptions.Count);
            Assert.AreEqual(2, subscriptions.First().StreamingServiceId);
        }

        [Test]
        public void UpdateUserSubscriptions_NonExistentUser_DoesNotThrowException()
        {
            int userId = 99;
            var prices = new Dictionary<int, decimal> { { 1, 0m } };
            Assert.DoesNotThrow(() => _repository.UpdateUserSubscriptions(userId, prices));
        }

    }

    // User Controller Tests
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ISubscriptionRepository> _subscriptionServiceMock;
        private Mock<ITitleRepository> _titleRepositoryMock;
        private UserController _controller;
        private DashboardModelView _dashboard;

        [SetUp]
        public void Setup()
        {
            var dummyLogger = Mock.Of<ILogger<UserController>>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _subscriptionServiceMock = new Mock<ISubscriptionRepository>();
            _titleRepositoryMock = new Mock<ITitleRepository>();

            _titleRepositoryMock
                .Setup(x => x.GetRecentlyViewedByUser(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<Title>());

            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptionRecords(It.IsAny<int>()))
                .Returns(new List<UserStreamingService>());
            _subscriptionServiceMock
                .Setup(s => s.GetAllServices())
                .Returns(new List<StreamingService>());
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(It.IsAny<int>()))
                .Returns(new List<StreamingService>());
            _subscriptionServiceMock
                .Setup(s => s.MonthlySubscriptionClicks(It.IsAny<int>()))
                .Returns(new List<SubscriptionClickSummary>());
            _subscriptionServiceMock
                .Setup(s => s.LifetimeSubscriptionClicks(It.IsAny<int>()))
                .Returns(new List<SubscriptionClickSummary>());


            _controller = new UserController(
                dummyLogger,
                null,
                _userRepositoryMock.Object,
                _subscriptionServiceMock.Object,
                _titleRepositoryMock.Object
            );

            var httpContext = new DefaultHttpContext();
            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );

            _dashboard = new DashboardModelView();
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public void SaveSubscriptions_WithValidSelectedServices_ReturnsDashboardView()
        {
            int userId = 1;
            string selectedServices = "1,2";
            string servicePrices = "{}";
            var subscriptions = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" },
                new StreamingService { Name = "Hulu" }
            };
            var user = new User { Id = userId, FirstName = "TestUser" };

            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(userId, It.IsAny<Dictionary<int, decimal>>()));
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId)).Returns(subscriptions);
            _userRepositoryMock
                .Setup(r => r.GetUser(userId)).Returns(user);

            var result = _controller
                .SaveSubscriptions(userId, selectedServices, servicePrices) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Dashboard", result.ViewName);
            var model = result.Model as DashboardModelView;
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
            string servicePrices = "{}";

            var subscriptions = new List<StreamingService>
    {
        new StreamingService { Name = "Netflix" }
    };

            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(userId, It.IsAny<Dictionary<int, decimal>>()));
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId)).Returns(subscriptions);
            _userRepositoryMock
                .Setup(r => r.GetUser(userId)).Returns((User)null);

            var result = _controller
                .SaveSubscriptions(userId, selectedServices, servicePrices) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Dashboard", result.ViewName);
            var model = result.Model as DashboardModelView;
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
            string servicePrices = "{}";

            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(userId, It.IsAny<Dictionary<int, decimal>>()))
                .Throws(new Exception("Test exception"));
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId))
                .Returns(new List<StreamingService>());
            _userRepositoryMock
                .Setup(r => r.GetUser(userId))
                .Returns(new User { Id = userId, FirstName = "TestUser" });

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var result = _controller.SaveSubscriptions(userId, selectedServices, servicePrices) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("SubscriptionForm", result.ViewName);
            Assert.IsTrue(_controller.TempData.ContainsKey("Message"));
            Assert.AreEqual(
                "There was an issue managing your subscription. Please try again later.",
                _controller.TempData["Message"]
            );
        }

        [Test]
        public void SaveSubscriptions_Success_ShouldSetTempDataMessageAndReturnDashboardView()
        {
            int userId = 1;
            string selectedServices = "1,2,3";
            string servicePrices = "{}";
            var user = new User { Id = userId, FirstName = "TestUser" };

            _userRepositoryMock.Setup(r => r.GetUser(userId)).Returns(user);
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId))
                .Returns(new List<StreamingService>());
            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(userId, It.IsAny<Dictionary<int, decimal>>()));

            var result = _controller.SaveSubscriptions(userId, selectedServices, servicePrices) as ViewResult;

            Assert.AreEqual("Subscriptions managed successfully!", _controller.TempData["Message"]);
            Assert.IsNotNull(result, "Expected a ViewResult.");
            Assert.AreEqual("Dashboard", result.ViewName);
        }

        [Test]
        public void SaveSubscriptions_WithNullSelectedServices_ProcessesDeletion()
        {
            int userId = 1;
            string selectedServices = null;
            string servicePrices = "{}";

            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(
                    userId,
                    It.Is<Dictionary<int, decimal>>(dict => dict.Count == 0)))
                .Verifiable();
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId))
                .Returns(new List<StreamingService>());
            _userRepositoryMock
                .Setup(r => r.GetUser(userId))
                .Returns(new User { Id = userId, FirstName = "TestUser" });

            var result = _controller.SaveSubscriptions(userId, selectedServices, servicePrices) as ViewResult;  // ? changed

            Assert.IsNotNull(result, "Expected a ViewResult when selectedServices is null.");
            Assert.AreEqual("Dashboard", result.ViewName, "Expected the Dashboard view to be returned.");
            _subscriptionServiceMock.Verify();
        }

        [Test]
        public void SaveSubscriptions_WithWhitespaceOnlySelectedServices_ProcessesDeletion()
        {
            int userId = 1;
            string selectedServices = "  ";
            string servicePrices = "{}";

            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(
                    userId,
                    It.Is<Dictionary<int, decimal>>(dict => dict.Count == 0)))
                .Verifiable();
            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId))
                .Returns(new List<StreamingService>());
            _userRepositoryMock
                .Setup(r => r.GetUser(userId))
                .Returns(new User { Id = userId, FirstName = "TestUser" });

            var result = _controller.SaveSubscriptions(userId, selectedServices, servicePrices) as ViewResult;

            Assert.IsNotNull(result, "Expected a ViewResult when selectedServices is whitespace.");
            Assert.AreEqual("Dashboard", result.ViewName, "Expected the Dashboard view to be returned.");
            _subscriptionServiceMock.Verify();
        }

        [Test]
        public void Cancel_RedirectsToDashboard()
        {
            var result = _controller.Cancel() as RedirectToActionResult;

            Assert.IsNotNull(result, "Expected a RedirectToActionResult from Cancel action.");
            Assert.AreEqual("Dashboard", result.ActionName, "Cancel should redirect to the Dashboard action.");
        }

        [Test]
        public void Cancel_DoesNotSubmitFormChanges_UserSubscriptionsRemainUnchanged()
        {
            int userId = 1;
            var originalSubscriptions = new List<StreamingService>
    {
        new StreamingService { Id = 1, Name = "Netflix" },
        new StreamingService { Id = 2, Name = "Hulu" }
    };

            _subscriptionServiceMock
                .Setup(s => s.GetUserSubscriptions(userId))
                .Returns(originalSubscriptions);
            _subscriptionServiceMock
                .Setup(s => s.UpdateUserSubscriptions(
                    It.IsAny<int>(),
                    It.IsAny<Dictionary<int, decimal>>()))
                .Verifiable();

            var result = _controller.Cancel();

            _subscriptionServiceMock.Verify(
                s => s.UpdateUserSubscriptions(
                    It.IsAny<int>(),
                    It.IsAny<Dictionary<int, decimal>>()),
                Times.Never);

            var subscriptionsAfterCancel = _subscriptionServiceMock.Object.GetUserSubscriptions(userId);
            Assert.AreEqual(originalSubscriptions.Count, subscriptionsAfterCancel.Count);
        }
    }
}
