using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.EntityFrameworkCore;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class TitleRepository : Repository<Title>, ITitleRepository
    {
        private readonly DbSet<Title> _titles;
        private readonly UserDbContext _context;

        public TitleRepository(UserDbContext context) : base(context)
        {
            _titles = context.Titles;
            _context = context;
        }

        public void CaptureOrUpdate(Title title)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));

            var existingTitle = _titles.FirstOrDefault(t =>
                        t.TitleName.ToLower().Trim() == title.TitleName.ToLower().Trim()
                        && t.Year == title.Year);

            if (existingTitle != null)
            {
                existingTitle.LastUpdated = DateTime.UtcNow;
                _titles.Update(existingTitle);
                
            }
            else
            {
                title.LastUpdated = DateTime.UtcNow;
                _titles.Add(title);
            }

            _context.SaveChanges();
        }
    }
}
