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
            var userSubscriptions = _uss
                .Include(us => us.StreamingService)  
                .Where(us => us.UserId == userId)    
                .Select(us => us.StreamingService)   
                .ToList();                           

            return userSubscriptions;
        }

        private HashSet<int> GetUserExistingSubscriptions(int userId)
        {
            return _uss
                .Where(us => us.UserId == userId)
                .Select(us => us.StreamingServiceId)
                .ToHashSet();
        }

        public void AddUserSubscriptions(int userId, List<int> selectedServiceIds)
        {
            try
            {
                var userExists = _context.Users.Any(u => u.Id == userId);
                if (!userExists)
                {
                    throw new InvalidOperationException("User does not exist.");
                }
                if (selectedServiceIds == null || !selectedServiceIds.Any())
                {
                    return;
                }

                var existingSubscriptions = GetUserExistingSubscriptions(userId);

                var newSubscriptions = selectedServiceIds
                    .Where(id => !existingSubscriptions.Contains(id))
                    .Select(id => new UserStreamingService
                    {
                        UserId = userId,
                        StreamingServiceId = id
                    })
                    .ToList();

                if (newSubscriptions.Any())
                {
                    _uss.AddRange(newSubscriptions);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
