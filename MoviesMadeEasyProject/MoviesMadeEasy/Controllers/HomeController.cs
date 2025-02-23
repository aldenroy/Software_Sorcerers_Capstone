using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesMadeEasy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMovieService _movieService;

        public HomeController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> SearchMovies(string query, string sortBy, int? minYear, int? maxYear)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { });
                }

                var movies = await _movieService.SearchMoviesAsync(query);

                // Filter by minYear and maxYear
                if (minYear.HasValue)
                {
                    movies = movies.Where(m => m.ReleaseYear >= minYear.Value).ToList();
                }
                if (maxYear.HasValue)
                {
                    movies = movies.Where(m => m.ReleaseYear <= maxYear.Value).ToList();
                }

                // Sort movies
                movies = sortBy switch
                {
                    "titleAsc" => movies.OrderBy(m => m.Title).ToList(),
                    "titleDesc" => movies.OrderByDescending(m => m.Title).ToList(),
                    "yearAsc" => movies.OrderBy(m => m.ReleaseYear).ToList(),
                    "yearDesc" => movies.OrderByDescending(m => m.ReleaseYear).ToList(),
                    _ => movies
                };

                return Json(movies);
            }
            catch (Exception)
            {
                return Json(new { });
            }
        }
    }
}
