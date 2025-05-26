using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models;
using MoviesMadeEasy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class TitleRepository : Repository<Title>, ITitleRepository
    {
        private readonly DbSet<Title> _titles;
        private readonly DbSet<RecentlyViewedTitle> _rvt;
        private readonly UserDbContext _context;

        public TitleRepository(UserDbContext context) : base(context)
        {
            _titles = context.Titles;
            _rvt = context.RecentlyViewedTitles;
            _context = context;
        }

        public void RecordTitleView(Title title, int userId)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            var dbTitle = CheckTitleExists(title);
            RecordRecentlyViewed(dbTitle.Id, userId);

        }

        public List<Title> GetRecentlyViewedByUser(int userId, int count = 5)
        {
            return _context.RecentlyViewedTitles
                           .Where(rv => rv.UserId == userId)
                           .OrderByDescending(rv => rv.ViewedAt)
                           .Select(rv => rv.Title)
                           .Distinct()
                           .Take(count)
                           .ToList();
        }

        private Title CheckTitleExists(Title title)
        {
            var existingTitle = _titles.FirstOrDefault(t =>
                        t.TitleName.ToLower().Trim() == title.TitleName.ToLower().Trim()
                        && t.Year == title.Year);

            if (existingTitle != null)
            {
                existingTitle.PosterUrl = title.PosterUrl;
                existingTitle.Genres = title.Genres;
                existingTitle.Rating = title.Rating;
                existingTitle.Overview = title.Overview;
                existingTitle.StreamingServices = title.StreamingServices;
                existingTitle.LastUpdated = DateTime.UtcNow;
                return existingTitle;
            }
            else
            {
                title.LastUpdated = DateTime.UtcNow;
                _titles.Add(title);
                _context.SaveChanges();
                return title;
            }
        }

        private void RecordRecentlyViewed(int titleId, int userId)
        {
            var rv = _rvt.SingleOrDefault(x => x.UserId == userId && x.TitleId == titleId);
            if (rv != null)
            {
                rv.ViewedAt = DateTime.UtcNow;
                _rvt.Update(rv);
            }
            else
            {
                _rvt.Add(new RecentlyViewedTitle
                {
                    UserId = userId,
                    TitleId = titleId,
                    ViewedAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();
        }

        public void Delete(int titleId, int userId)
        {
            var rvt = _rvt.SingleOrDefault(x => x.UserId == userId && x.TitleId == titleId);
            if (rvt == null) return;

            _rvt.Remove(rvt);
            _context.SaveChanges();
        }
    }
}
