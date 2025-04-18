using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface ITitleRepository : IRepository<Title>
    {
        void CaptureOrUpdate(Title title);
    }
}
