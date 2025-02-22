using System.Collections.Generic;
using MoviesMadeEasy.Models;
using ShowCatalog.DAL.Abstract;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ISubscriptionRepository : IRepository<UserStreamingService>
    {
        List<StreamingService> GetUserSubscriptions(int userId);
    }
}
