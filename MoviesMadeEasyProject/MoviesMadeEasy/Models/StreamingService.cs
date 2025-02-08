using System;
using System.Collections.Generic;

namespace MoviesMadeEasy.Models;

public partial class StreamingService
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
