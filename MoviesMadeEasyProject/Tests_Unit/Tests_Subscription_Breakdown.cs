using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.DAL.Concrete;

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
    }
}
