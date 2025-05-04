using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Moq;
using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using MME_Tests;

namespace Tests_Unit
{
    [TestFixture]
    internal class Tests_Subscription_Breakdown
    {
        private Mock<UserDbContext> _mockContext;
        private SubscriptionRepository _repo;
        private IQueryable<StreamingService> _streamingServices;
        private IQueryable<UserStreamingService> _userStreamingServices;

        [SetUp]
        public void Setup()
        {
            _streamingServices = new List<StreamingService>
            {
                new StreamingService { Id = 1, Name = "Netflix", MonthlyCost = 9.99m },
                new StreamingService { Id = 2, Name = "Amazon Prime", MonthlyCost = null },
                new StreamingService { Id = 3, Name = "Hulu", MonthlyCost = 5.00m }
            }.AsQueryable();

            _userStreamingServices = new List<UserStreamingService>
            {
                new UserStreamingService { UserId = 42, StreamingServiceId = 1 },
                new UserStreamingService { UserId = 42, StreamingServiceId = 2 }
            }.AsQueryable();

            var mockSvcSet = MockHelper.GetMockDbSet(_streamingServices);
            var mockUsrSet = MockHelper.GetMockDbSet(_userStreamingServices);

            _mockContext = new Mock<UserDbContext>();
            _mockContext.Setup(c => c.StreamingServices).Returns(mockSvcSet.Object);
            _mockContext.Setup(c => c.UserStreamingServices).Returns(mockUsrSet.Object);

            _repo = new SubscriptionRepository(_mockContext.Object);
        }

        [TestCase(null, true)]
        [TestCase(0.00, true)]
        [TestCase(1000.00, true)]
        [TestCase(-0.01, false)]
        [TestCase(1000.01, false)]
        public void StreamingService_MonthlyCostValidation(decimal? cost, bool expectedIsValid)
        {
            var svc = new StreamingService
            {
                Id = 1,
                Name = "Test",
                MonthlyCost = cost
            };

            var context = new ValidationContext(svc);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(svc, context, results, true);

            Assert.AreEqual(expectedIsValid, isValid);
        }

        [Test]
        public void CalculateTotalMonthlyCost_SumsOnlySubscribedCosts()
        {
            decimal total = _repo.CalculateTotalMonthlyCost(42);
            Assert.AreEqual(9.99m, total);
        }
    }
}
