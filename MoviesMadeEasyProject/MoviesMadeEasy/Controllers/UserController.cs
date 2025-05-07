using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models.ModelView;
using Newtonsoft.Json;

namespace MoviesMadeEasy.Controllers
{
    public class UserController : BaseController
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ISubscriptionRepository _subscriptionService;
        private readonly ITitleRepository _titleRepository;

        public UserController(
            ILogger<UserController> logger,
            UserManager<IdentityUser> userManager,
            IUserRepository userRepository,
            ISubscriptionRepository subscriptionService,
            ITitleRepository titleRepository)
            : base(userManager, userRepository, logger)
        {
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
            _subscriptionService = subscriptionService;
            _titleRepository = titleRepository;
        }

        private DashboardModelView BuildDashboardModelView(int userId)
        {
            var user = _userRepository.GetUser(userId);
            var userSubscriptions = _subscriptionService.GetUserSubscriptions(userId);
            var allServices = _subscriptionService.GetAllServices()?.ToList() ?? new List<StreamingService>();
            var recentTitles = _titleRepository
                      .GetRecentlyViewedByUser(userId)
                      .OrderByDescending(tv => tv.LastUpdated)
                      .Take(10)
                      .ToList();
            var rawSubs = _subscriptionService.GetUserSubscriptionRecords(userId);
            var priceLookup = rawSubs
                .ToDictionary(us => us.StreamingServiceId, us => us.MonthlyCost);
            var total = _subscriptionService.GetUserSubscriptionTotalMonthlyCost(userId);
            return new DashboardModelView
            {
                UserId = userId,
                UserName = user != null ? user.FirstName : "",
                HasSubscriptions = userSubscriptions != null && userSubscriptions.Any(),
                SubList = userSubscriptions?.ToList() ?? new List<StreamingService>(),
                AllServicesList = allServices,
                PreSelectedServiceIds = userSubscriptions != null
                                        ? string.Join(",", userSubscriptions.Select(s => s.Id))
                                        : "",
               RecentlyViewedTitles = recentTitles,
               ServicePrices = priceLookup,
               TotalMonthlyCost = total
            };
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
                var dto = BuildDashboardModelView(user.Id);
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard for user");
                return RedirectToAction("Error");
            }
        }

        public IActionResult SubscriptionForm(DashboardModelView dto)
        {
            var updatedDto = BuildDashboardModelView(dto.UserId);
            return View(updatedDto);
        }

        [HttpPost]
        public IActionResult SaveSubscriptions(int userId, string selectedServices, string servicePrices)
        {
            selectedServices = selectedServices ?? "";
            servicePrices = servicePrices ?? "{}";

            var selectedIds = String.IsNullOrWhiteSpace(selectedServices)
              ? new List<int>()
              : selectedServices
                  .Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => int.Parse(s.Trim()))
                  .ToList();

            Dictionary<int, decimal> priceDict =
                JsonConvert.DeserializeObject<Dictionary<int, decimal>>(servicePrices)
                ?? new Dictionary<int, decimal>();

            try
            {
                _subscriptionService.UpdateUserSubscriptions(userId, priceDict);

                TempData["Message"] = "Subscriptions managed successfully!";
                var dto = BuildDashboardModelView(userId);
                return View("Dashboard", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving subscriptions for userId: {userId}", userId);
                TempData["Message"] = "There was an issue managing your subscription. Please try again later.";
                var dto = BuildDashboardModelView(userId);
                return View("SubscriptionForm", dto);
            }
        }

        [HttpDelete("User/RemoveRecentlyViewed/{titleId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveRecentlyViewed(int titleId)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null) return Unauthorized();

            var user = _userRepository.GetUser(identityUser.Id);   
            _titleRepository.Delete(titleId, user.Id);

            return Ok();                                           
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
