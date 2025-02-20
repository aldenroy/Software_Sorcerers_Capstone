using System;

namespace MoviesMadeEasy.Models;

public class UserStreamingService
{
    public string UserId { get; set; }
    public virtual User User { get; set; }

    public Guid StreamingServiceId { get; set; }
    public virtual StreamingService StreamingService { get; set; }
}
