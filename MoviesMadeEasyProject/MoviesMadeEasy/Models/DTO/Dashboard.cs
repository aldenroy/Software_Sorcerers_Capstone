namespace MoviesMadeEasy.DTOs
{
    public class DashboardDTO
    {
        public string UserName { get; set; } 
        public bool HasSubscriptions { get; set; } 
        public List<string> SubscriptionNames { get; set; }
        public List<string> SubscriptionLogos { get; set; }
    }
}
