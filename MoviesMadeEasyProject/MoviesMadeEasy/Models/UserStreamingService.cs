using System;

namespace MoviesMadeEasy.Models;

public class UserStreamingService
{
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public int StreamingServiceId { get; set; }
    public decimal? MonthlyCost { get; set; }
    public virtual StreamingService StreamingService { get; set; }
}
