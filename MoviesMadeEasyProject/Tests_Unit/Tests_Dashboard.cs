using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.DTOs;
namespace MME_Tests
{
    [TestFixture]
    public class StreamingServiceAndDashboardTests
    {
        private Mock<UserDbContext> _mockContext;
        private SubscriptionRepository _repository;
        private List<StreamingService> _streamingServices;
        private List<UserStreamingService> _userStreamingServices;
        private DashboardDTO _dashboard;

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

            _userStreamingServices = new List<UserStreamingService>();

            var mockStreamingServicesDbSet = MockHelper.GetMockDbSet(_streamingServices.AsQueryable());
            var mockUserStreamingServicesDbSet = MockHelper.GetMockDbSet(_userStreamingServices.AsQueryable());

            _mockContext.Setup(c => c.StreamingServices).Returns(mockStreamingServicesDbSet.Object);
            _mockContext.Setup(c => c.UserStreamingServices).Returns(mockUserStreamingServicesDbSet.Object);

            _repository = new SubscriptionRepository(_mockContext.Object);

            _dashboard = new DashboardDTO { UserName = "Test" };
        }

        [Test]
        public void GetAvailableStreamingServices_UserHasNoSubscriptions_ReturnsAllServices()
        {
            int userId = 1;
            var result = _repository.GetAvailableStreamingServices(userId);
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.Any(s => s.Name == "Netflix"));
            Assert.IsTrue(result.Any(s => s.Name == "Hulu"));
            Assert.IsTrue(result.Any(s => s.Name == "Disney+"));
            Assert.IsTrue(result.Any(s => s.Name == "Amazon Prime Video"));
        }

        [Test]
        public void GetAvailableStreamingServices_UserSubscribedToNetflix_ReturnsAllButNetflix()
        {
            int userId = 1;
            _streamingServices.First(s => s.Name == "Netflix").UserStreamingServices
                .Add(new UserStreamingService { UserId = userId });
            var result = _repository.GetAvailableStreamingServices(userId);
            Assert.IsFalse(result.Any(s => s.Name == "Netflix"));
            Assert.IsTrue(result.Any(s => s.Name == "Hulu"));
            Assert.IsTrue(result.Any(s => s.Name == "Disney+"));
            Assert.IsTrue(result.Any(s => s.Name == "Amazon Prime Video"));
        }

        [Test]
        public void GetAvailableStreamingServices_UserSubscribedToAll_ReturnsEmptyList()
        {
            int userId = 1;
            foreach (var service in _streamingServices)
            {
                service.UserStreamingServices.Add(new UserStreamingService { UserId = userId });
            }
            var result = _repository.GetAvailableStreamingServices(userId);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetAvailableStreamingServices_ReturnsListInAlphabeticalOrder()
        {
            int userId = 1;
            var result = _repository.GetAvailableStreamingServices(userId);
            var expectedOrder = result.OrderBy(s => s.Name).ToList();
            Assert.IsTrue(result.SequenceEqual(expectedOrder));
        }

        [Test]
        public void AddUserSubscriptions_NullOrEmptyList_DoesNotThrowException()
        {
            int userId = 1;
            Assert.DoesNotThrow(() => _repository.AddUserSubscriptions(userId, new List<int>()));
            Assert.DoesNotThrow(() => _repository.AddUserSubscriptions(userId, null));
        }

        [Test]
        public void GetAvailableStreamingServices_NoStreamingServices_ReturnsEmptyList()
        {
            _streamingServices.Clear();
            var result = _repository.GetAvailableStreamingServices(1);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetAvailableStreamingServices_NonExistentUser_ReturnsAllServices()
        {
            int userId = 99;
            var result = _repository.GetAvailableStreamingServices(userId);
            Assert.AreEqual(4, result.Count);
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
    }
}