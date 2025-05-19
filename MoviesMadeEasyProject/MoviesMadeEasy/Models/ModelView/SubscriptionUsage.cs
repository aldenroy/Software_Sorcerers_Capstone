namespace MoviesMadeEasy.Models.ModelView
{
    public class SubscriptionUsageModelView
    {
        public int StreamingServiceId { get; set; }
        public string ServiceName { get; set; }
        public int MonthlyClicks { get; set; }
        public int LifetimeClicks { get; set; }
    }
}
