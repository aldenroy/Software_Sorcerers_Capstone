using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace MoviesMadeEasy.Models;

public partial class StreamingService
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

}
