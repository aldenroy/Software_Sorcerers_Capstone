using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.DTOs
{
    public class DashboardDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } 
        public bool HasSubscriptions { get; set; } 
        public List<StreamingService> SubList { get; set; }
    }
}
