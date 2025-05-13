using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesMadeEasy.Models;

public class UserStreamingService
{
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public int StreamingServiceId { get; set; }
    [Range(typeof(decimal), "0.00", "1000.00", ErrorMessage = "Monthly cost must be between {1} and {2}.")]
    public decimal? MonthlyCost { get; set; }
    public int ClickCount { get; set; }
    public virtual StreamingService StreamingService { get; set; }
}
