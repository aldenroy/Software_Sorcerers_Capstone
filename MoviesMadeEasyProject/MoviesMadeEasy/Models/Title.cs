using System;
using System.Collections.Generic;

namespace MoviesMadeEasy.Models;

public partial class Title
{
    public Guid Id { get; set; }

    public string? ExternalId { get; set; }

    public string TitleName { get; set; } = null!;

    public int Year { get; set; }

    public string Type { get; set; } = null!;

    public DateTime LastUpdated { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
