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
            //_homeController = new HomeController(_mockMovieService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _homeController?.Dispose(); // Ensuring proper cleanup
        }

        [Test]
        public async Task SearchMovies_ValidQuery_ReturnsJsonResult()
        {
            // Arrange
            var query = "Inception";
            var movies = new List<Movie> { new Movie { Title = "Inception", ReleaseYear = 2010 } };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            // Act
            var result = await _homeController.SearchMovies(query, null, null, null) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            Assert.That(result.Value, Is.EqualTo(movies));
        }

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
        // Kept these tests for sort by in this file because it's under the same controller and mock
        // without it I get an ambiguity error. Subset of the same feature so we'll keep them here.
        public async Task SearchMovies_SortByTitleAsc_ReturnsSortedMoviesAscending()
        {
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Zodiac", ReleaseYear = 2007 },
                new Movie { Title = "Inception", ReleaseYear = 2010 },
                new Movie { Title = "Batman Begins", ReleaseYear = 2005 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            var result = await _homeController.SearchMovies(query, "titleAsc", null, null) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var sortedMovies = result.Value as List<Movie>;
            Assert.That(sortedMovies, Is.Ordered.By(nameof(Movie.Title)));
        }

        [Test]
        public async Task SearchMovies_SortByTitleDesc_ReturnsSortedMoviesDescending()
        {
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Zodiac", ReleaseYear = 2007 },
                new Movie { Title = "Inception", ReleaseYear = 2010 },
                new Movie { Title = "Batman Begins", ReleaseYear = 2005 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            var result = await _homeController.SearchMovies(query, "titleDesc", null, null) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var sortedMovies = result.Value as List<Movie>;
            Assert.That(sortedMovies, Is.Ordered.By(nameof(Movie.Title)).Descending);
        }

        [Test]
        public async Task SearchMovies_SortByYearAsc_ReturnsMoviesSortedByYearAscending()
        {
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Interstellar", ReleaseYear = 2014 },
                new Movie { Title = "The Matrix", ReleaseYear = 1999 },
                new Movie { Title = "The Dark Knight", ReleaseYear = 2008 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            var result = await _homeController.SearchMovies(query, "yearAsc", null, null) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var sortedMovies = result.Value as List<Movie>;
            Assert.That(sortedMovies, Is.Ordered.By(nameof(Movie.ReleaseYear)));
        }

        [Test]
        public async Task SearchMovies_SortByYearDesc_ReturnsMoviesSortedByYearDescending()
        {
            var query = "Movie";
            var movies = new List<Movie>
            {
                new Movie { Title = "Interstellar", ReleaseYear = 2014 },
                new Movie { Title = "The Matrix", ReleaseYear = 1999 },
                new Movie { Title = "The Dark Knight", ReleaseYear = 2008 }
            };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            var result = await _homeController.SearchMovies(query, "yearDesc", null, null) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var sortedMovies = result.Value as List<Movie>;
            Assert.That(sortedMovies, Is.Ordered.By(nameof(Movie.ReleaseYear)).Descending);
        }
    }
}
