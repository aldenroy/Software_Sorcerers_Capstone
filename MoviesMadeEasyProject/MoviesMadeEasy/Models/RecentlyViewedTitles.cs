namespace MoviesMadeEasy.Models;

public partial class RecentlyViewedTitle
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public Guid TitleId { get; set; }
    public DateTime ViewedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Title Title { get; set; } = null!;
}