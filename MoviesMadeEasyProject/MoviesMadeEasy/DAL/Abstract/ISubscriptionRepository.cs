﻿using MoviesMadeEasy.Models;
using MoviesMadeEasy.Models.DTO;


namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ISubscriptionRepository : IRepository<UserStreamingService>
    {
        IEnumerable<StreamingService> GetAllServices();
        List<StreamingService> GetUserSubscriptions(int userId);
        void UpdateUserSubscriptions(int userId, Dictionary<int, decimal> priceDict);
        public List<UserStreamingService> GetUserSubscriptionsWithCost(int userId);
        List<UserStreamingService> GetUserSubscriptionRecords(int userId);
        decimal GetUserSubscriptionTotalMonthlyCost(int userId);
        Task IncrementClickCountAsync(int userId, int streamingServiceId);
        List<SubscriptionClickSummary> MonthlySubscriptionClicks(int userId);
        List<SubscriptionClickSummary> LifetimeSubscriptionClicks(int userId);
    }
}
