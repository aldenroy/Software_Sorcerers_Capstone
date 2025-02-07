using MoviesMadeEasy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface IMovieService
    {
        Task<List<Movie>> SearchMoviesAsync(string query);
    }
}
