using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly UserDbContext _context;

        public SubscriptionService(UserDbContext context)
        {
            _context = context;
        }

        public List<StreamingService> GetUserSubscriptions(string userId)
        {
            return _context.UserStreamingServices
                .Where(uss => uss.UserId == userId)
                .Include(uss => uss.StreamingService)
                .Select(uss => uss.StreamingService)
                .ToList();
        }
    }
}
