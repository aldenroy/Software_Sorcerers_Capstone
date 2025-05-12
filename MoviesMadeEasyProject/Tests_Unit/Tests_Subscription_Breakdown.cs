using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.DAL.Concrete;
using System.ComponentModel.DataAnnotations;

namespace MME_Tests
{
    [TestFixture]
    public class TogglePriceSubscriptionTests
    {
        private UserDbContext _db;
        private SubscriptionRepository _repo;
        private int _userId = 42;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new UserDbContext(options);

            _db.StreamingServices.AddRange(
                new StreamingService { Id = 1, Name = "Hulu" },
                new StreamingService { Id = 2, Name = "Disney+" },
                new StreamingService { Id = 3, Name = "Netflix" },
                new StreamingService { Id = 4, Name = "Paramount" }
            );

            _db.Users.Add(new User
            {
                Id = _userId,
                AspNetUserId = Guid.NewGuid().ToString(),
                FirstName = "Test",
                LastName = "User",
                ColorMode = "Light",
                FontSize = "Medium",
                FontType = "Sans-serif"
            });

            _db.UserStreamingServices.AddRange(
                new UserStreamingService { UserId = _userId, StreamingServiceId = 1, MonthlyCost = 10m },
                new UserStreamingService { UserId = _userId, StreamingServiceId = 2, MonthlyCost = 15m },
                new UserStreamingService { UserId = _userId, StreamingServiceId = 3, MonthlyCost = 20m }
            );

            _db.SaveChanges();

            _repo = new SubscriptionRepository(_db);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        private static IList<ValidationResult> ValidateProperty(object instance, string propName)
        {
            var context = new ValidationContext(instance) { MemberName = propName };
            var results = new List<ValidationResult>();
            var value = instance.GetType()
                                .GetProperty(propName)
                                .GetValue(instance);
            Validator.TryValidateProperty(value, context, results);
            return results;
        }

        [Test]
        public void UpdateUserSubscriptions_AddsNew_RemovesOld_And_UpdatesPrices()
        {
            var newPrices = new Dictionary<int, decimal>
            {
                [2] = 17.50m,
                [4] = 25.00m
            };

            _repo.UpdateUserSubscriptions(_userId, newPrices);

            var subs = _db.UserStreamingServices
                .Where(us => us.UserId == _userId)
                .OrderBy(us => us.StreamingServiceId)
                .ToList();

            Assert.False(subs.Any(us => us.StreamingServiceId == 1));
            var two = subs.Single(us => us.StreamingServiceId == 2);
            Assert.AreEqual(17.50m, two.MonthlyCost);
            Assert.False(subs.Any(us => us.StreamingServiceId == 3));
            var four = subs.Single(us => us.StreamingServiceId == 4);
            Assert.AreEqual(25.00m, four.MonthlyCost);
        }

        [Test]
        public void UpdateSubscriptionMonthlyCost_ExistingSubscription_UpdatesAndSaves()
        {
            _repo.UpdateSubscriptionMonthlyCost(_userId, 3, 11.11m);

            var sub = _db.UserStreamingServices
                .Single(us => us.UserId == _userId && us.StreamingServiceId == 3);

            Assert.AreEqual(11.11m, sub.MonthlyCost);
        }

        [Test]
        public void UpdateSubscriptionMonthlyCost_Nonexistent_DoesNothing()
        {
            Assert.DoesNotThrow(() =>
                _repo.UpdateSubscriptionMonthlyCost(_userId, 99, 5.55m)
            );

            var count = _db.UserStreamingServices.Count(us => us.UserId == _userId);
            Assert.AreEqual(3, count);
        }

        [Test]
        public void GetUserSubscriptionTotalMonthlyCost_ReturnsSumOfMonthlyCosts()
        {
            // 10 + 15 + 20 = 45
            var total = _repo.GetUserSubscriptionTotalMonthlyCost(_userId);
            Assert.AreEqual(45m, total);
        }

        [Test]
        public void GetUserSubscriptionTotalMonthlyCost_TreatsNullCostAsZero()
        {
            _db.UserStreamingServices.Add(new UserStreamingService
            {
                UserId = _userId,
                StreamingServiceId = 4,
                MonthlyCost = null
            });
            _db.SaveChanges();

            // original sum is 45, null should be treated as 0
            var total = _repo.GetUserSubscriptionTotalMonthlyCost(_userId);
            Assert.AreEqual(45m, total);
        }

        [Test]
        public void GetUserSubscriptionTotalMonthlyCost_NoSubscriptions_ReturnsZero()
        {
            const int otherUser = 99;
            var total = _repo.GetUserSubscriptionTotalMonthlyCost(otherUser);
            Assert.AreEqual(0m, total);
        }

        [TestCase(-0.01)]
        [TestCase(-50)]
        [TestCase(1000.01)]
        [TestCase(1500)]
        public void MonthlyCost_OutOfRange_FailsValidation(decimal cost)
        {
            var svc = new UserStreamingService { MonthlyCost = cost };
            var errors = ValidateProperty(svc, nameof(svc.MonthlyCost));
            Assert.IsNotEmpty(errors, $"Expected validation to fail for cost {cost}");
        }

        [TestCase(0.00)]
        [TestCase(0.01)]
        [TestCase(500.00)]
        [TestCase(1000.00)]
        public void MonthlyCost_InRange_PassesValidation(decimal cost)
        {
            var svc = new UserStreamingService { MonthlyCost = cost };
            var errors = ValidateProperty(svc, nameof(svc.MonthlyCost));
            Assert.IsEmpty(errors, $"Expected validation to pass for cost {cost}");
        }

        [Test]
        public void MonthlyCost_Null_PassesValidation()
        {
            var svc = new UserStreamingService { MonthlyCost = null };
            var errors = ValidateProperty(svc, nameof(svc.MonthlyCost));
            Assert.IsEmpty(errors, "Null MonthlyCost should be valid.");
        }
    }
}
