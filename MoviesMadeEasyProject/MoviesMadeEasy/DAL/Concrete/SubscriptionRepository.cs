using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using Microsoft.EntityFrameworkCore;
using ShowCatalog.DAL.Abstract;
using ShowCatalog.DAL.Concrete;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class SubscriptionRepository : Repository<UserStreamingService>, ISubscriptionRepository
    {
        private readonly DbSet<UserStreamingService> _uss;

        public SubscriptionRepository(UserDbContext context) : base(context)
        {
            _uss = context.UserStreamingServices;
        }

        public List<StreamingService> GetUserSubscriptions(int userId)
        {
            // Query to get all streaming services for a specific user
            var userSubscriptions = _uss
                .Include(us => us.StreamingService)  // Include the related StreamingService
                .Where(us => us.UserId == userId)    // Filter by user ID
                .Select(us => us.StreamingService)   // Select only the StreamingService part
                .ToList();                           

            return userSubscriptions;
        }

        public List<StreamingService> GetAvailableStreamingServices(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
