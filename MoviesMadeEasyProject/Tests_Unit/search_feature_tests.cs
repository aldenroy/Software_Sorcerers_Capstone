using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.Controllers;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MME_Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IMovieService> _mockMovieService;
        private HomeController _homeController;

        [SetUp]
        public void Setup()
        {
            _mockMovieService = new Mock<IMovieService>();

            // Mock other dependencies required by HomeController
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            var mockUserRepository = new Mock<IUserRepository>();
            var mockLogger = new Mock<ILogger<BaseController>>();

            // Initialize the HomeController with mocked dependencies
            _homeController = new HomeController(
                _mockMovieService.Object,
                mockUserManager.Object,
                mockUserRepository.Object,
                mockLogger.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _homeController?.Dispose(); // Ensuring proper cleanup
        }

        //checking that no query returns an empty results
        [Test]
        public async Task SearchMovies_EmptyQuery_ReturnsEmptyJsonObject()
        {
            var query = "";

            var result = await _homeController.SearchMovies(query, null, null, null) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonResult.Count == 0);
        }

        //throwing error if no results possible from query title
        [Test]
        public async Task SearchMovies_ServiceThrowsException_ReturnsEmptyJsonObject()
        {
            var query = "Test Movie";
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ThrowsAsync(new Exception("API error"));

            var result = await _homeController.SearchMovies(query, null, null, null) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonResult.Count == 0);
        }

        [Test]
        public async Task SearchMovies_SortByTitleAsc_ReturnsSortedMoviesAscending()
        {
            // Arrange
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Zodiac", ReleaseYear = 2007 },
                new Movie { Title = "Inception", ReleaseYear = 2010 },
                new Movie { Title = "Batman Begins", ReleaseYear = 2005 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            // Act
            var result = await _homeController.SearchMovies(query, "titleAsc", null, null) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var movieResults = result.Value as IEnumerable<object>;
            Assert.IsNotNull(movieResults);

            // Deserialize to the structured object list we need
            var movieList = movieResults.Select(m => JObject.FromObject(m)).ToList();

            var sortedTitles = movieList.Select(m => m["title"]?.ToString()).ToList();
            Assert.That(sortedTitles, Is.Ordered.Ascending);
        }

        [Test]
        public async Task SearchMovies_SortByTitleDesc_ReturnsSortedMoviesDescending()
        {
            // Arrange
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Zodiac", ReleaseYear = 2007 },
                new Movie { Title = "Inception", ReleaseYear = 2010 },
                new Movie { Title = "Batman Begins", ReleaseYear = 2005 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            // Act
            var result = await _homeController.SearchMovies(query, "titleDesc", null, null) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var movieResults = result.Value as IEnumerable<object>;
            Assert.IsNotNull(movieResults);

            var movieList = movieResults.Select(m => JObject.FromObject(m)).ToList();

            var sortedTitles = movieList.Select(m => m["title"]?.ToString()).ToList();
            Assert.That(sortedTitles, Is.Ordered.Descending);
        }

        [Test]
        public async Task SearchMovies_SortByYearAsc_ReturnsMoviesSortedByYearAscending()
        {
            // Arrange
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Interstellar", ReleaseYear = 2014 },
                new Movie { Title = "The Matrix", ReleaseYear = 1999 },
                new Movie { Title = "The Dark Knight", ReleaseYear = 2008 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            // Act
            var result = await _homeController.SearchMovies(query, "yearAsc", null, null) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var movieResults = result.Value as IEnumerable<object>;
            Assert.IsNotNull(movieResults);

            var movieList = movieResults.Select(m => JObject.FromObject(m)).ToList();

            var sortedYears = movieList.Select(m => (int)m["releaseYear"]).ToList();
            Assert.That(sortedYears, Is.Ordered.Ascending);
        }

        [Test]
        public async Task SearchMovies_SortByYearDesc_ReturnsMoviesSortedByYearDescending()
        {
            // Arrange
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Interstellar", ReleaseYear = 2014 },
                new Movie { Title = "The Matrix", ReleaseYear = 1999 },
                new Movie { Title = "The Dark Knight", ReleaseYear = 2008 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            // Act
            var result = await _homeController.SearchMovies(query, "yearDesc", null, null) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var movieResults = result.Value as IEnumerable<object>;
            Assert.IsNotNull(movieResults);

            var movieList = movieResults.Select(m => JObject.FromObject(m)).ToList();

            var sortedYears = movieList.Select(m => (int)m["releaseYear"]).ToList();
            Assert.That(sortedYears, Is.Ordered.Descending);
        }
    }
}