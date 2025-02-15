using MoviesMadeEasy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MoviesMadeEasy.DAL.Abstract
{
    public interface IMovieService
    {
        Task<List<Movie>> SearchMoviesAsync(string query);
    }
}
