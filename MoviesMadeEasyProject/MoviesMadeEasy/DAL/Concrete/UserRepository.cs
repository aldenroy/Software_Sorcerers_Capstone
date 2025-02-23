using Microsoft.EntityFrameworkCore;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.DAL.Concrete;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly DbSet<User> _users;
    public UserRepository(UserDbContext context) : base(context)
    {
        _users = context.Users;
    }
    public User GetUser(string aspNetUserId)
    {
        var user = _users.FirstOrDefault(u => u.AspNetUserId == aspNetUserId);
        return user;
    }
}