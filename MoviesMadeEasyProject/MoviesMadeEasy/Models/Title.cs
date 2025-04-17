namespace MoviesMadeEasy.Models;

public partial class Title
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public string TitleName { get; set; } = null!;
    public int Year { get; set; }
    public string Type { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public string? Genres { get; set; }
    public string? Rating { get; set; }
    public string? Overview { get; set; }
    public string? StreamingServices { get; set; }
    public DateTime LastUpdated { get; set; }

    public virtual ICollection<RecentlyViewedTitle> RecentlyViewedTitles { get; set; }
        = new List<RecentlyViewedTitle>();
}