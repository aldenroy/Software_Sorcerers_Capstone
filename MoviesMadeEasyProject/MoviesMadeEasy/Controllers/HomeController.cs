using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MoviesMadeEasy.DTOs;
using Microsoft.AspNetCore.Identity;
using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<User> _userManager;

        public HomeController(IMovieService movieService, ISubscriptionService subscriptionService, UserManager<User> userManager)
        {
            _movieService = movieService;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userId = user.Id;
            var userSubscriptions = _subscriptionService.GetUserSubscriptions(userId);

            var dto = new DashboardDTO
            {
                UserName = userId,
                HasSubscriptions = userSubscriptions != null && userSubscriptions.Any(),
                SubscriptionNames = userSubscriptions?.Select(s => s.Name).ToList() ?? new List<string>(),
                SubscriptionLogos = userSubscriptions?.Select(s => s.LogoUrl).ToList() ?? new List<string>()
            };

            return View(dto);
        }

        [HttpGet]
        public async Task<JsonResult> SearchMovies(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { }); // Ensure consistent return type
                }

                var movies = await _movieService.SearchMoviesAsync(query);
                return Json(movies);
            }
            catch (Exception)
            {
                return Json(new { }); // Handle errors gracefully
            }
        }
    }
}