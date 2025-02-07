using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.DAL.Abstract;
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
        public async Task<JsonResult> SearchMovies(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { }); // Ensure consistent return type
                }

                var movies = await _movieService.SearchMoviesAsync(query);
                return Json(movies);
            }
            catch (Exception)
            {
                return Json(new { }); // Handle errors gracefully
            }
        }

    }
}