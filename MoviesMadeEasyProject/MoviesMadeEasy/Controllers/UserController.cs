using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MoviesMadeEasy.DTOs;
using MoviesMadeEasy.DAL.Abstract;

namespace MoviesMadeEasy.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ISubscriptionRepository _subscriptionService;

        public UserController(
            ILogger<UserController> logger,
            UserManager<IdentityUser> userManager,
            IUserRepository userRepository,
            ISubscriptionRepository subscriptionService)
        {
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
            _subscriptionService = subscriptionService;
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var identityUser = await _userManager.GetUserAsync(User);
                if (identityUser == null)
                {
                    return Unauthorized();
                }

                var user = _userRepository.GetUser(identityUser.Id);
                var userSubscriptions = _subscriptionService.GetUserSubscriptions(user.Id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var dto = new DashboardDTO
                {
                    UserId = user.Id,
                    UserName = $"{user.FirstName}",
                    HasSubscriptions = userSubscriptions != null && userSubscriptions.Any(),
                    SubList = userSubscriptions?.ToList() ?? new List<StreamingService>()
                };

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard for user");
                return RedirectToAction("Error");
            }
        }
        public IActionResult AddSubscriptionForm(int userId)
        {
            var availableServices = _subscriptionService.GetAvailableStreamingServices(userId);
            return View(availableServices);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}