using NUnit.Framework;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Moq;
using System.Linq.Expressions;

namespace MoviesMadeEasy.Tests
{
    [TestFixture]
    public class SubscriptionRepositoryTests
    {
        private Mock<UserDbContext> _contextMock;
        private Mock<DbSet<StreamingService>> _streamingServicesMock;
        private Mock<DbSet<UserStreamingService>> _userStreamingServicesMock;
        private SubscriptionRepository _repository;
        private List<StreamingService> _seedData;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _seedData = new List<StreamingService>
            {
                new StreamingService 
                { 
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"), 
                    Name = "Netflix", 
                    Region = "US", 
                    BaseUrl = "https://www.netflix.com/login", 
                    LogoUrl = "/images/Netflix_Symbol_RGB.png" 
                },
                new StreamingService 
                { 
                    Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), 
                    Name = "Hulu", 
                    Region = "US", 
                    BaseUrl = "https://auth.hulu.com/web/login", 
                    LogoUrl = "/images/hulu-Green-digital.png" 
                }
            };
        }

        [SetUp]
        public void Setup()
        {
            var queryableData = _seedData.AsQueryable();

            _streamingServicesMock = new Mock<DbSet<StreamingService>>();
            _streamingServicesMock.As<IQueryable<StreamingService>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            _streamingServicesMock.As<IQueryable<StreamingService>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            _streamingServicesMock.As<IQueryable<StreamingService>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            _streamingServicesMock.As<IQueryable<StreamingService>>().Setup(m => m.GetEnumerator()).Returns(() => queryableData.GetEnumerator());

            _userStreamingServicesMock = new Mock<DbSet<UserStreamingService>>();
            var userStreamingServices = new List<UserStreamingService>().AsQueryable();
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.Provider).Returns(userStreamingServices.Provider);
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.Expression).Returns(userStreamingServices.Expression);
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.ElementType).Returns(userStreamingServices.ElementType);
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.GetEnumerator()).Returns(() => userStreamingServices.GetEnumerator());

            _contextMock = new Mock<UserDbContext>();
            _contextMock.Setup(c => c.StreamingServices).Returns(_streamingServicesMock.Object);
            _contextMock.Setup(c => c.UserStreamingServices).Returns(_userStreamingServicesMock.Object);

            _repository = new SubscriptionRepository(_contextMock.Object);
        }

        [Test]
        public void GetAvailableStreamingServices_ReturnsAllServices()
        {
            var services = _repository.GetAvailableStreamingServices(1);

            Assert.That(services, Is.Not.Null);
            Assert.That(services.Count, Is.EqualTo(_seedData.Count));
        }

        [Test]
        public void GetAvailableStreamingServices_ReturnsCorrectData()
        {
            var services = _repository.GetAvailableStreamingServices(1);

            foreach (var expectedService in _seedData)
            {
                var actualService = services.FirstOrDefault(s => s.Id == expectedService.Id);
                Assert.That(actualService, Is.Not.Null, $"Service with ID {expectedService.Id} not found");
                Assert.Multiple(() =>
                {
                    Assert.That(actualService.Name, Is.EqualTo(expectedService.Name));
                    Assert.That(actualService.Region, Is.EqualTo(expectedService.Region));
                    Assert.That(actualService.BaseUrl, Is.EqualTo(expectedService.BaseUrl));
                    Assert.That(actualService.LogoUrl, Is.EqualTo(expectedService.LogoUrl));
                });
            }
        }

        [Test]
        public void GetAvailableStreamingServices_ReturnsServicesInAlphabeticalOrder()
        {
            var services = _repository.GetAvailableStreamingServices(1);

            var sortedNames = services.Select(s => s.Name).ToList();
            var expectedOrder = sortedNames.OrderBy(n => n).ToList();
            Assert.That(sortedNames, Is.EqualTo(expectedOrder));
        }

        [Test]
        public void GetAvailableStreamingServices_ExcludesExistingSubscriptions()
        {
            var userId = 1;
            var existingService = _seedData.First();
            var userStreamingServices = new List<UserStreamingService> 
            { 
                new UserStreamingService 
                { 
                    UserId = userId,
                    StreamingServiceId = existingService.Id
                }
            }.AsQueryable();

            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.Provider).Returns(userStreamingServices.Provider);
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.Expression).Returns(userStreamingServices.Expression);
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.ElementType).Returns(userStreamingServices.ElementType);
            _userStreamingServicesMock.As<IQueryable<UserStreamingService>>().Setup(m => m.GetEnumerator()).Returns(() => userStreamingServices.GetEnumerator());

            var services = _repository.GetAvailableStreamingServices(userId);

            Assert.That(services.Count, Is.EqualTo(_seedData.Count - 1));
            Assert.That(services.Any(s => s.Id == existingService.Id), Is.False);
        }
    }
}