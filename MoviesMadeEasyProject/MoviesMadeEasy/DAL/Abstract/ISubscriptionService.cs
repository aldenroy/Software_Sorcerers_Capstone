using System.Collections.Generic;
using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ISubscriptionService
    {
        List<StreamingService> GetUserSubscriptions(string userId);
    }
}
