using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.DTOs
{
    public class DashboardDTO
    {
        public string UserName { get; set; } 
        public bool HasSubscriptions { get; set; } 
        public List<StreamingService> SubList { get; set; }
    }
}
