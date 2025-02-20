using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.Models;
using Microsoft.AspNetCore.Identity;

namespace MoviesMadeEasy.Controllers;

public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;
    public UserController(ILogger<UserController> logger, SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Login(string email, string password, string returnUrl)
    {
        return View(); // Temporary placeholder to make the test fail properly
    }


    public IActionResult Register()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
