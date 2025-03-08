using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesMadeEasy.Models
{
    public class User
    {
        public int Id { get; set; }

        public string AspNetUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid? RecentlyViewedShowId { get; set; }

        public string ColorMode { get; set; } = "Light";
        public string FontSize { get; set; } = "Medium";
        public string FontType { get; set; } = "Standard";

        public virtual Title RecentlyViewedShow { get; set; }
        public virtual ICollection<UserStreamingService> UserStreamingServices { get; set; }
    }
}
