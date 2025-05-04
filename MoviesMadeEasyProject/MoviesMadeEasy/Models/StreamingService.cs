using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace MoviesMadeEasy.Models;
using System.ComponentModel.DataAnnotations;

public partial class StreamingService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public string? BaseUrl { get; set; }

    public string? LogoUrl { get; set; }

    [Range(0.00, 1000.00, ErrorMessage = "Monthly cost must be between 0 and 1000.")]
    public decimal? MonthlyCost { get; set; }

    public virtual ICollection<UserStreamingService> UserStreamingServices { get; set; } = new List<UserStreamingService>();

}


