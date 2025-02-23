using MoviesMadeEasy.Models;


namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ISubscriptionRepository : IRepository<UserStreamingService>
    {
        List<StreamingService> GetUserSubscriptions(int userId);
        List<StreamingService> GetAvailableStreamingServices(int userId);
    }
}
