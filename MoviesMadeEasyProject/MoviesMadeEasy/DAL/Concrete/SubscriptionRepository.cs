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

        private List<UserStreamingService> GetUserSubscriptionRecords(int userId)
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

        private void AddNewSubscriptions(int userId, List<int> selectedServiceIds)
        {
            var currentSubscriptions = GetUserSubscriptionRecords(userId);
            var currentSubscriptionIds = currentSubscriptions
                .Select(us => us.StreamingServiceId)
                .ToHashSet();

            var subscriptionsToAdd = selectedServiceIds
                .Where(id => !currentSubscriptionIds.Contains(id))
                .Select(id => new UserStreamingService
                {
                    UserId = userId,
                    StreamingServiceId = id
                })
                .ToList();

            if (subscriptionsToAdd.Any())
            {
                _uss.AddRange(subscriptionsToAdd);
            }
        }

        public void UpdateUserSubscriptions(int userId, List<int> selectedServiceIds)
        {
            RemoveUnselectedSubscriptions(userId, selectedServiceIds);
            AddNewSubscriptions(userId, selectedServiceIds);
            _context.SaveChanges();
        }

    }
}
