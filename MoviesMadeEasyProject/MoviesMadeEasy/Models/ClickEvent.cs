namespace MoviesMadeEasy.Models
{
    public class ClickEvent
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StreamingServiceId { get; set; }
        public DateTime ClickedAt { get; set; }
        public virtual User User { get; set; }
        public virtual StreamingService StreamingService { get; set; }
    }
}
