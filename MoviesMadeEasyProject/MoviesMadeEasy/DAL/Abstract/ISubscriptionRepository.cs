using MoviesMadeEasy.Models;


namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ISubscriptionRepository : IRepository<UserStreamingService>
    {
        IEnumerable<StreamingService> GetAllServices();
        List<StreamingService> GetUserSubscriptions(int userId);
        void UpdateUserSubscriptions(int userId, Dictionary<int, decimal> priceDict);
    }
}
