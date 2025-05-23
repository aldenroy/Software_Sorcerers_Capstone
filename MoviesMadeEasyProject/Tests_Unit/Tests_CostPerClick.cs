using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.DAL.Concrete;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Models.ModelView;

namespace MME_Tests
{
    [TestFixture]
    public class CostPerClickTests
    {
        private UserDbContext _db;
        private SubscriptionRepository _repo;
        private const int UserWithMultiple = 42;

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

            _db.Users.Add(new User { Id = UserWithMultiple, AspNetUserId = Guid.NewGuid().ToString(), FirstName = "Test", LastName = "User" });

            _db.UserStreamingServices.AddRange(
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 1, MonthlyCost = 10m },
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 2, MonthlyCost = 20m },
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 3, MonthlyCost = null },
                new UserStreamingService { UserId = UserWithMultiple, StreamingServiceId = 4, MonthlyCost = 40m }
            );

            _db.SaveChanges();
            _repo = new SubscriptionRepository(_db);
        }

        [TearDown]
        public void TearDown() => _db.Dispose();

        private SubscriptionUsageModelView GetUsageSummary(int serviceId, params (int DaysAgo, int Count)[] clicks)
        {
            var now = DateTime.Now;
            foreach (var (DaysAgo, Count) in clicks)
            {
                var events = Enumerable.Range(1, Count)
                    .Select(_ => new ClickEvent
                    {
                        UserId = UserWithMultiple,
                        StreamingServiceId = serviceId,
                        ClickedAt = now.AddDays(-DaysAgo)
                    });
                _db.ClickEvents.AddRange(events);
            }
            _db.SaveChanges();

            var monthClicks = _repo.MonthlySubscriptionClicks(UserWithMultiple);
            var lifetimeClicks = _repo.LifetimeSubscriptionClicks(UserWithMultiple);
            var priceLookup = _db.UserStreamingServices
                                     .Where(us => us.UserId == UserWithMultiple)
                                     .ToDictionary(us => us.StreamingServiceId, us => us.MonthlyCost);

            var usageSummaries = monthClicks
                .GroupJoin(
                    lifetimeClicks,
                    m => m.StreamingServiceId,
                    l => l.StreamingServiceId,
                    (m, lGroup) => {
                        var price = priceLookup.GetValueOrDefault(m.StreamingServiceId);
                        var costPerClick = (m.ClickCount > 0 && price.HasValue)
                                            ? price.Value / m.ClickCount
                                            : (decimal?)null;
                        return new SubscriptionUsageModelView
                        {
                            StreamingServiceId = m.StreamingServiceId,
                            ServiceName = m.ServiceName,
                            MonthlyClicks = m.ClickCount,
                            LifetimeClicks = lGroup.Select(l => l.ClickCount).FirstOrDefault(),
                            MonthlyCost = price,
                            CostPerClick = costPerClick
                        };
                    })
                .ToList();

            return usageSummaries.Single(s => s.StreamingServiceId == serviceId);
        }

        [Test]
        public void Calculates_CostPerClick_When_Clicks_And_Price()
        {
            var summary = GetUsageSummary(serviceId: 1, (DaysAgo: 3, Count: 2));
            Assert.AreEqual(2, summary.MonthlyClicks);
            Assert.AreEqual(10m, summary.MonthlyCost);
            Assert.AreEqual(5m, summary.CostPerClick);
        }

        [Test]
        public void Returns_Null_CostPerClick_When_No_Clicks()
        {
            var summary = GetUsageSummary(serviceId: 2);
            Assert.AreEqual(0, summary.MonthlyClicks);
            Assert.IsNull(summary.CostPerClick);
        }

        [Test]
        public void Returns_Null_CostPerClick_When_No_Price()
        {
            var summary = GetUsageSummary(serviceId: 3, (DaysAgo: 1, Count: 3));
            Assert.AreEqual(3, summary.MonthlyClicks);
            Assert.IsNull(summary.MonthlyCost);
            Assert.IsNull(summary.CostPerClick);
        }


        [Test]
        public void CostPerClick_Uses_Only_MonthlyClicks_Not_Lifetime()
        {
            var summary = GetUsageSummary(serviceId: 1, (DaysAgo: 40, Count: 1), (DaysAgo: 5, Count: 1));
            Assert.AreEqual(1, summary.MonthlyClicks, "should only count clicks in the last 30 days");
            Assert.AreEqual(2, summary.LifetimeClicks, "lifetime clicks includes both recent and old");
            Assert.AreEqual(10m, summary.MonthlyCost);
            Assert.AreEqual(10m, summary.CostPerClick, "cost per click should be price / monthly clicks only");
        }
    }
}
