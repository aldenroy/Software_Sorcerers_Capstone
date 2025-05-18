using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using NUnit.Framework;

namespace MME_Tests
{
    [TestFixture]
    public class CostBreakdownTests
    {
        private UserDbContext _db;
        private SubscriptionRepository _repo;
        private const int UserWithMultiple = 42;
        private const int UserWithOne = 99;

        [SetUp]
        public void SetUp()
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
                Id = UserWithMultiple,
                AspNetUserId = Guid.NewGuid().ToString(),
                FirstName = "Test",
                LastName = "User",
                ColorMode = "Light",
                FontSize = "Medium",
                FontType = "Sans-serif"
            });

            _db.UserStreamingServices.AddRange(
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 1, MonthlyCost = 10m },
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 2, MonthlyCost = 15m },
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 3, MonthlyCost = 20m }
            );

            _db.Users.Add(new User
            {
                Id = UserWithOne,
                AspNetUserId = Guid.NewGuid().ToString(),
                FirstName = "Click",
                LastName = "Tester",
                ColorMode = "Light",
                FontSize = "Medium",
                FontType = "Sans-serif"
            });
            _db.UserStreamingServices.Add(
                new UserStreamingService { UserId = UserWithOne, StreamingServiceId = 1, MonthlyCost = 5m }
            );

            _db.SaveChanges();
            _repo = new SubscriptionRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

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

        // --- Tests for UpdateUserSubscriptions, UpdateSubscriptionMonthlyCost, GetUserSubscriptionTotalMonthlyCost ---

        [Test]
        public void UpdateUserSubscriptions_AddsNew_RemovesOld_And_UpdatesPrices()
        {
            var newPrices = new Dictionary<int, decimal>
            {
                [2] = 17.50m,
                [4] = 25.00m
            };

            _repo.UpdateUserSubscriptions(UserWithMultiple, newPrices);

            var subs = _db.UserStreamingServices
                .Where(us => us.UserId == UserWithMultiple)
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
            _repo.UpdateSubscriptionMonthlyCost(UserWithMultiple, 3, 11.11m);

            var sub = _db.UserStreamingServices
                .Single(us => us.UserId == UserWithMultiple && us.StreamingServiceId == 3);

            Assert.AreEqual(11.11m, sub.MonthlyCost);
        }

        [Test]
        public void UpdateSubscriptionMonthlyCost_Nonexistent_DoesNothing()
        {
            Assert.DoesNotThrow(() =>
                _repo.UpdateSubscriptionMonthlyCost(UserWithMultiple, 99, 5.55m)
            );

            var count = _db.UserStreamingServices.Count(us => us.UserId == UserWithMultiple);
            Assert.AreEqual(3, count);
        }

        [Test]
        public void GetUserSubscriptionTotalMonthlyCost_ReturnsSumOfMonthlyCosts()
        {
            // 10 + 15 + 20 = 45
            var total = _repo.GetUserSubscriptionTotalMonthlyCost(UserWithMultiple);
            Assert.AreEqual(45m, total);
        }

        [Test]
        public void GetUserSubscriptionTotalMonthlyCost_TreatsNullCostAsZero()
        {
            _db.UserStreamingServices.Add(new UserStreamingService
            {
                UserId = UserWithMultiple,
                StreamingServiceId = 4,
                MonthlyCost = null
            });
            _db.SaveChanges();

            var total = _repo.GetUserSubscriptionTotalMonthlyCost(UserWithMultiple);
            Assert.AreEqual(45m, total);
        }

        [Test]
        public void GetUserSubscriptionTotalMonthlyCost_NoSubscriptions_ReturnsZero()
        {
            const int otherUser = 1234;
            var total = _repo.GetUserSubscriptionTotalMonthlyCost(otherUser);
            Assert.AreEqual(0m, total);
        }

        // --- Validation tests for MonthlyCost ---

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

        // --- Click-count tests originally in SubscriptionRepositoryClickCountTests ---

        [Test]
        public void SeededSubscriptionsStartWithZeroClicks()
        {
            var subs = _repo.GetUserSubscriptionRecords(UserWithOne);
            Assert.That(subs, Is.Not.Empty);
            Assert.That(subs.Select(x => x.ClickCount), Is.All.EqualTo(0));
        }

        [Test]
        public async Task IncrementClickCountAsync_IncrementsProperly()
        {
            var svcId = _repo.GetUserSubscriptionRecords(UserWithOne).First().StreamingServiceId;
            await _repo.IncrementClickCountAsync(UserWithOne, svcId);

            var click = _repo.GetUserSubscriptionRecords(UserWithOne)
                            .First(x => x.StreamingServiceId == svcId)
                            .ClickCount;
            Assert.AreEqual(1, click);
        }

        [Test]
        public async Task IncrementClickCountAsync_Nonexistent_DoesNotThrowOrAffect()
        {
            var before = _repo.GetUserSubscriptionRecords(UserWithOne).Sum(x => x.ClickCount);
            await _repo.IncrementClickCountAsync(UserWithOne, 999);
            var after = _repo.GetUserSubscriptionRecords(UserWithOne).Sum(x => x.ClickCount);
            Assert.AreEqual(before, after);
        }
    }
}
