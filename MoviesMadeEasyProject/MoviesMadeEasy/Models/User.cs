using System;
using System.Collections.Generic;

namespace MoviesMadeEasy.Models;

public partial class User
{
    public Guid AspnetIdentityId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Guid? StreamingServicesId { get; set; }

    public Guid? RecentlyViewedShowId { get; set; }

    public virtual Title? RecentlyViewedShow { get; set; }

    public virtual StreamingService? StreamingServices { get; set; }
}
