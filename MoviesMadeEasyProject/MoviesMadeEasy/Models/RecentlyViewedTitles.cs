namespace MoviesMadeEasy.Models;

public partial class RecentlyViewedTitle
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TitleId { get; set; }
    public DateTime ViewedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Title Title { get; set; } = null!;
}