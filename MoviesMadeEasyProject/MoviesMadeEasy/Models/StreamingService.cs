using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace MoviesMadeEasy.Models;

public partial class StreamingService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public string? BaseUrl { get; set; }

    public string? LogoUrl { get; set; }

    public virtual ICollection<UserStreamingService> UserStreamingServices { get; set; } = new List<UserStreamingService>();

}


