using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ITitleRepository : IRepository<Title>
    {
        void RecordTitleView(Title title, int userId);
        List<Title> GetRecentlyViewedByUser(int userId, int count = 10);
    }
}
