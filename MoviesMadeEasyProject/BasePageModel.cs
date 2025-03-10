using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;


public class BasePageModel : PageModel
{
    protected readonly UserManager<User> _userManager;

    public BasePageModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public string Theme { get; private set; } = "Light"; // Default

    public async Task OnGetThemeAsync()
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["Theme"] = user?.ColorMode?.ToLower() ?? "light";
        }
        else
        {
            ViewData["Theme"] = "light"; 
        }
    }
}
