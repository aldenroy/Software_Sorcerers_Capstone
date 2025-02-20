using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
namespace MoviesMadeEasy.Models;

public partial class User : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public Guid? RecentlyViewedShowId { get; set; }

    public virtual Title? RecentlyViewedShow { get; set; }

    public virtual ICollection<UserStreamingService> UserStreamingServices { get; set; } = new List<UserStreamingService>();

    public string ColorMode { get; set; } = "Light"; // Default to Light

    public string FontSize { get; set; } = "Medium"; // Default to Medium
    
    public string FontType { get; set; } = "Standard"; // Default to Standard
}
