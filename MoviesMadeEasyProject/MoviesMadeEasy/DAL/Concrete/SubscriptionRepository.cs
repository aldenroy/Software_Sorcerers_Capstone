﻿using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class SubscriptionRepository : Repository<UserStreamingService>, ISubscriptionRepository
    {
        private readonly DbSet<UserStreamingService> _uss;
        private readonly DbSet<StreamingService> _streamingServices;
        private UserDbContext _context;

        public List<StreamingService> StreamingServices { get; }

        public SubscriptionRepository(UserDbContext context) : base(context)
        {
            _uss = context.UserStreamingServices;
            _streamingServices = context.StreamingServices;
            _context = context;
        }


        public List<StreamingService> GetUserSubscriptions(int userId)
        {
            // Query to get all streaming services for a specific user
            var userSubscriptions = _uss
                .Include(us => us.StreamingService)  
                .Where(us => us.UserId == userId)    
                .Select(us => us.StreamingService)   
                .ToList();                           

            return userSubscriptions;
        }

        public List<StreamingService> GetAvailableStreamingServices(int userId)
        {
            var toAddSubsList = _streamingServices
                .Include(ss => ss.UserStreamingServices)
                .Where(ss => ss.UserStreamingServices.All(us => us.UserId != userId))
                .OrderBy(ss => ss.Name)
                .ToList();

            return toAddSubsList;
        }

        public void AddUserSubscriptions(int userId, List<int> selectedServiceIds)
        {
            var userExists = _context.Users.Any(u => u.Id == userId);
            if (!userExists)
            {
                throw new InvalidOperationException("User does not exist.");
            }

            var newSubscriptions = selectedServiceIds
                .Select(id => new UserStreamingService
                {
                    UserId = userId,
                    StreamingServiceId = id
                })
                .ToList();

            _uss.AddRange(newSubscriptions);
            _context.SaveChanges();
        }
    }
}
