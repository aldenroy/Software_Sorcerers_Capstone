using MoviesMadeEasy.Models;
using MoviesMadeEasy.DAL.Abstract;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface IUserRepository : IRepository<User>
    {
        User GetUser(string aspNetUserId);
        User GetUser(int userId);

    }
}
