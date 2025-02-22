using MoviesMadeEasy.Models;
using ShowCatalog.DAL.Abstract;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface IUserRepository : IRepository<User>
    {
        User GetUser(string aspNetUserId);

    }
}
