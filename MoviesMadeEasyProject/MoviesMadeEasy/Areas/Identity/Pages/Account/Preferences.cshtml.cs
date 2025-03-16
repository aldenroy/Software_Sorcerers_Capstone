using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MoviesMadeEasy.Data;
using MoviesMadeEasy.Models;
using Microsoft.EntityFrameworkCore;


namespace MoviesMadeEasy.Areas.Identity.Pages.Account
{
    public class RegisterPreferencesModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserDbContext _userContext;

        public RegisterPreferencesModel(
            UserManager<IdentityUser> userManager,
            UserDbContext userContext)
        {
            _userManager = userManager;
            _userContext = userContext;
            Input = new InputModel();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string ColorMode { get; set; }
            public string FontSize { get; set; }
            public string FontType { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var customUser = await _userContext.Users
                .FirstOrDefaultAsync(u => u.AspNetUserId == user.Id);

            if (customUser != null)
            {
                customUser.ColorMode = Input.ColorMode;
                customUser.FontSize = Input.FontSize;
                customUser.FontType = Input.FontType;
                await _userContext.SaveChangesAsync();
                ViewData["ColorMode"] = Input.ColorMode.ToLower();
            }

            return RedirectToAction("Dashboard", "User");
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var customUser = await _userContext.Users
                .FirstOrDefaultAsync(u => u.AspNetUserId == user.Id);

            if (customUser != null)
            {
                // Load the user's existing preferences
                Input.ColorMode = customUser.ColorMode;
                Input.FontSize = customUser.FontSize;
                Input.FontType = customUser.FontType;

                ViewData["ColorMode"] = Input.ColorMode;
            }

            return Page();
        }

        public IActionResult OnPostSkip()
        {
            return RedirectToAction("Dashboard", "User");
        }

    }
}
