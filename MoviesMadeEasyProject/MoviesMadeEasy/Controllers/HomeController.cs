using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesMadeEasy.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IMovieService _movieService;
        private readonly ILogger<BaseController> _logger;

        public HomeController(IMovieService movieService, ILogger<BaseController> logger) : base(null, null, logger) // To be changed if future features require HomeController to use UserManager or IUserRepository
        {
            _movieService = movieService;
            _logger = logger;
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
                    "yearAsc"         => movies.OrderBy(m => m.ReleaseYear).ToList(),
                    "yearDesc"        => movies.OrderByDescending(m => m.ReleaseYear).ToList(),
                    "titleAsc"        => movies.OrderBy(m => m.Title).ToList(),
                    "titleDesc"       => movies.OrderByDescending(m => m.Title).ToList(),
                    "ratingHighLow"   => movies.OrderByDescending(m => m.Rating).ToList(),
                    "ratingLowHigh"   => movies.OrderBy(m => m.Rating).ToList(),
                    _                 => movies
                };

                var movieResults = movies.Select(movie => new 
                {
                    title = movie.Title,
                    releaseYear = movie.ReleaseYear,
                    posterUrl = movie.ImageSet?.VerticalPoster?.W240 ?? "https://via.placeholder.com/150", // Example fallback if null
                    genres = movie.Genres?.Select(g => g.Name).ToList() ?? new List<string>(), // Handle null genres
                    rating = movie.Rating,
                    overview = movie.Overview,
                    services = movie.StreamingOptions?
                        .SelectMany(kvp => kvp.Value) // Flatten the lists of StreamingOption
                        .Select(option => option.Service?.Name) // Select the Name property from each Service
                        .Where(name => name != null) // Filter out null values (if any)
                        .Distinct() // Remove duplicates (if needed)
                        .ToList() ?? new List<string>() // Handle null StreamingOptions
                }).ToList();

                
                return Json(movieResults);
            }
            catch (Exception)
            {
                return Json(new { });
            }
        }
    }
}
