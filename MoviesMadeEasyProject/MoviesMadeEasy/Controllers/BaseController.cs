using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesMadeEasy.Controllers
{
    public class BaseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public BaseController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected async Task<string> GetUserThemeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.ColorMode ?? "Light"; // Default to Light mode
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            ViewBag.Theme = await GetUserThemeAsync();
            await next();
        }
    }

}