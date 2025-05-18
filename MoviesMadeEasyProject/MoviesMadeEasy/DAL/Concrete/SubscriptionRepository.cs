using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class SubscriptionRepository : Repository<UserStreamingService>, ISubscriptionRepository
    {
        private readonly DbSet<UserStreamingService> _uss;
        private readonly DbSet<StreamingService> _streamingServices;
        private readonly UserDbContext _context;

        public SubscriptionRepository(UserDbContext context) : base(context)
        {
            _uss = context.UserStreamingServices;
            _streamingServices = context.StreamingServices;
            _context = context;
        }

        public IEnumerable<StreamingService> GetAllServices()
        {
            return _streamingServices.OrderBy(s => s.Name).ToList();
        }

        public List<StreamingService> GetUserSubscriptions(int userId)
        {
            return _uss
                .Include(us => us.StreamingService)
                .Where(us => us.UserId == userId)
                .Select(us => us.StreamingService)
                .ToList();
        }

        public List<UserStreamingService> GetUserSubscriptionsWithCost(int userId)
        {
            return _uss
                .Where(us => us.UserId == userId)
                .ToList();
        }

        public List<UserStreamingService> GetUserSubscriptionRecords(int userId)
        {
            return _context.UserStreamingServices
                .Where(us => us.UserId == userId)
                .ToList();
        }

        private void RemoveUnselectedSubscriptions(int userId, List<int> selectedServiceIds)
        {
            var currentSubscriptions = GetUserSubscriptionRecords(userId);
            var subscriptionsToRemove = currentSubscriptions
                .Where(us => !selectedServiceIds.Contains(us.StreamingServiceId))
                .ToList();

            if (subscriptionsToRemove.Any())
            {
                _uss.RemoveRange(subscriptionsToRemove);
            }
        }

        private void AddNewSubscriptions(int userId,
                                         Dictionary<int, decimal> servicePrices)
        {
            var existing = GetUserSubscriptionRecords(userId)
                .Select(us => us.StreamingServiceId)
                .ToHashSet();

            var toAdd = servicePrices.Keys
                .Where(id => !existing.Contains(id))
                .Select(id => new UserStreamingService
                {
                    UserId = userId,
                    StreamingServiceId = id,
                    MonthlyCost = servicePrices[id]
                });

            _uss.AddRange(toAdd);
        }


        public void UpdateUserSubscriptions(int userId, Dictionary<int, decimal> servicePrices)
        {
            var selectedIds = servicePrices.Keys.ToList();

            RemoveUnselectedSubscriptions(userId, selectedIds);
            AddNewSubscriptions(userId, servicePrices);
            UpdateSubscriptionPrices(userId, servicePrices);

            _context.SaveChanges();
        }

        private void UpdateSubscriptionPrices(int userId, Dictionary<int, decimal> servicePrices)
        {
            var current = GetUserSubscriptionRecords(userId)
                .Where(us => servicePrices.ContainsKey(us.StreamingServiceId));

            foreach (var sub in current)
            {
                sub.MonthlyCost = servicePrices[sub.StreamingServiceId];
            }
        }

        public void UpdateSubscriptionMonthlyCost(int userId, int streamingServiceId, decimal monthlyCost)
        {
            var subscription = _uss
                .FirstOrDefault(us => us.UserId == userId && us.StreamingServiceId == streamingServiceId);

            if (subscription != null)
            {
                subscription.MonthlyCost = monthlyCost;
                _context.SaveChanges();
            }
        }

        public decimal GetUserSubscriptionTotalMonthlyCost(int userId)
        {
            return _uss
                .Where(us => us.UserId == userId)
                .Select(us => us.MonthlyCost ?? 0m)
                .Sum();
        }

        public async Task IncrementClickCountAsync(int userId, int streamingServiceId)
        {
            var sub = await _uss
                .FirstOrDefaultAsync(us => us.UserId == userId && us.StreamingServiceId == streamingServiceId);

            if (sub != null)
            {
                sub.ClickCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}
