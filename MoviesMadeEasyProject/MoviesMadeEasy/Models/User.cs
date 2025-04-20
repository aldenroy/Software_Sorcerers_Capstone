namespace MoviesMadeEasy.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string AspNetUserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string ColorMode { get; set; } = "Light";
        public string FontSize { get; set; } = "Medium";
        public string FontType { get; set; } = "Standard";

        public virtual ICollection<UserStreamingService> UserStreamingServices { get; set; }
            = new List<UserStreamingService>();

        public virtual ICollection<RecentlyViewedTitle> RecentlyViewedTitles { get; set; }
            = new List<RecentlyViewedTitle>();
    }
}
