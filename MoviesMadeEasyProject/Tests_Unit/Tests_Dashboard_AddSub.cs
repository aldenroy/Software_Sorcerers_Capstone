using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;


namespace MME_Tests
{
    [TestFixture]
    public class StreamingServiceTests
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

            // Create mock user subscriptions (empty at first)
            _userStreamingServices = new List<UserStreamingService>();

            // Mock DbSet for StreamingServices
            var mockStreamingServicesDbSet = GetMockDbSet(_streamingServices.AsQueryable());

            // Mock DbSet for UserStreamingServices
            var mockUserStreamingServicesDbSet = GetMockDbSet(_userStreamingServices.AsQueryable());

            // Mock UserDbContext
            _mockContext.Setup(c => c.StreamingServices).Returns(mockStreamingServicesDbSet.Object);
            _mockContext.Setup(c => c.UserStreamingServices).Returns(mockUserStreamingServicesDbSet.Object);

            // Inject mock context into repository
            _repository = new SubscriptionRepository(_mockContext.Object);
        }

        private static Mock<DbSet<T>> GetMockDbSet<T>(IQueryable<T> entities) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(entities.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entities.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entities.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(entities.GetEnumerator());
            return mockSet;
        }

        [Test]
        public void Dashboard_GetAvailableStreamingServices_UserHasNoSubscriptions_ReturnsAllServices()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = _repository.GetAvailableStreamingServices(userId);

            // Assert
            Assert.AreEqual(4, result.Count);
            Assert.IsTrue(result.Any(s => s.Name == "Netflix"));
            Assert.IsTrue(result.Any(s => s.Name == "Hulu"));
            Assert.IsTrue(result.Any(s => s.Name == "Disney+"));
            Assert.IsTrue(result.Any(s => s.Name == "Amazon Prime Video"));
        }


        [Test]
        public void GetAvailableStreamingServices_UserSubscribedToNetflix_ReturnsAllButNetflix()
        {
            // Arrange
            int userId = 1;
            _streamingServices.First(s => s.Name == "Netflix").UserStreamingServices
                .Add(new UserStreamingService { UserId = userId });

            // Act
            var result = _repository.GetAvailableStreamingServices(userId);

            // Assert
            Assert.IsFalse(result.Any(s => s.Name == "Netflix"));
            Assert.IsTrue(result.Any(s => s.Name == "Hulu"));
            Assert.IsTrue(result.Any(s => s.Name == "Disney+"));
            Assert.IsTrue(result.Any(s => s.Name == "Amazon Prime Video"));
        }

        [Test]
        public void GetAvailableStreamingServices_ReturnsListInAlphabeticalOrder()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = _repository.GetAvailableStreamingServices(userId);
            var expectedOrder = result.OrderBy(s => s.Name).ToList();

            // Assert
            Assert.IsTrue(result.SequenceEqual(expectedOrder));
        }
    }
}