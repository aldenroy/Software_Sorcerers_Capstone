using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.Models.ModelView
{
    public class DashboardModelView
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool HasSubscriptions { get; set; }
        public List<StreamingService> SubList { get; set; }
        public List<StreamingService> AllServicesList { get; set; }
        public string PreSelectedServiceIds { get; set; }

    }
}
