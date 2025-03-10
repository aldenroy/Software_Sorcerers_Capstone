using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using System;
using System.Linq;
using System.Threading.Tasks;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesMadeEasy.Controllers
{
    public class BaseController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<BaseController> _logger;

        public BaseController(UserManager<IdentityUser> userManager, IUserRepository userRepository, ILogger<BaseController> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Set default theme
            ViewData["ColorMode"] = "light";
            
            try
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    _logger.LogInformation("User is authenticated, fetching color mode preference");
                    
                    var identityUser = await _userManager.GetUserAsync(User);
                    if (identityUser != null)
                    {
                        _logger.LogInformation($"Identity user found: {identityUser.Id}");
                        
                        try
                        {
                            var user = _userRepository.GetUser(identityUser.Id);
                            if (user != null)
                            {
                                // Normalize the color mode value
                                string colorMode = !string.IsNullOrWhiteSpace(user.ColorMode) 
                                    ? user.ColorMode.ToLower().Trim() 
                                    : "light";
                                    
                                // Ensure the value is either "light" or "dark"
                                if (colorMode != "light" && colorMode != "dark")
                                {
                                    colorMode = "light"; // Default to light if invalid value
                                }
                                
                                ViewData["ColorMode"] = colorMode;
                                _logger.LogInformation($"Set color mode to: {colorMode}");
                            }
                            else
                            {
                                _logger.LogWarning("User object from repository is null");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error retrieving user from repository");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Identity user is null");
                    }
                }
                else
                {
                    _logger.LogInformation("User is not authenticated, using default light mode");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting color mode");
            }

            // Continue with the action execution
            await next();
        }
    }
}