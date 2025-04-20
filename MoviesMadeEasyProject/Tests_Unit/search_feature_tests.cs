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
using System.Net; // Added for HttpStatusCode
using Microsoft.AspNetCore.Http;

namespace MME_Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IOpenAIService> _mockOpenAIService;
        private Mock<IMovieService> _mockMovieService;
        private HomeController _homeController;
        private Mock<ITitleRepository> _mockTitleRepo;

        [SetUp]
        public void Setup()
        {
            _mockOpenAIService = new Mock<IOpenAIService>();
            _mockMovieService = new Mock<IMovieService>();
            _mockTitleRepo = new Mock<ITitleRepository>();

            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            var mockUserRepository = new Mock<IUserRepository>();
            var mockLogger = new Mock<ILogger<BaseController>>();

            _homeController = new HomeController(
                _mockOpenAIService.Object,
                _mockMovieService.Object,
                mockUserManager.Object,
                mockUserRepository.Object,
                _mockTitleRepo.Object,
                mockLogger.Object
            );

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            mockSession.Setup(_ => _.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => {
                    writer.Write(value);
                    writer.Flush();
                });

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _homeController.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };
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


        //START of openi tests. In same file due to mocking conflict
        [Test]
        public async Task GetSimilarMovies_ValidTitle_ReturnsRecommendations()
        {
            // Arrange
            var testTitle = "Inception";
            var expectedRecommendations = new List<MovieRecommendation>
            {
                new MovieRecommendation { Title = "The Matrix", Year = 1999 },
                new MovieRecommendation { Title = "Interstellar", Year = 2014 }
            };

            _mockOpenAIService.Setup(s => s.GetSimilarMoviesAsync(testTitle))
                .ReturnsAsync(expectedRecommendations);

            // Mock session
            var session = new Mock<ISession>();
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session.Object }
            };
            _homeController.ControllerContext = controllerContext;

            // Act
            var result = await _homeController.GetSimilarMovies(testTitle) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(expectedRecommendations, result.Value);
            
            // Verify session was set
            session.Verify(s => s.Set(
                "LastRecommendations", 
                It.IsAny<byte[]>()), Times.Once);
            session.Verify(s => s.Set(
                "LastRecommendationTitle", 
                It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public async Task GetSimilarMovies_RateLimitExceeded_Returns429()
        {
            // Arrange
            var testTitle = "Inception";
            _mockOpenAIService.Setup(s => s.GetSimilarMoviesAsync(testTitle))
                .ThrowsAsync(new HttpRequestException("Rate limit exceeded", null, HttpStatusCode.TooManyRequests));

            // Act
            var result = await _homeController.GetSimilarMovies(testTitle);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var statusResult = result as ObjectResult;
            Assert.AreEqual(429, statusResult.StatusCode);
            Assert.IsNotNull(statusResult.Value);
        }

        [Test]
        public async Task GetSimilarMovies_ServiceError_Returns500()
        {
            // Arrange
            var testTitle = "Inception";
            _mockOpenAIService.Setup(s => s.GetSimilarMoviesAsync(testTitle))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _homeController.GetSimilarMovies(testTitle);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var statusResult = result as ObjectResult;
            Assert.AreEqual(500, statusResult.StatusCode);
            Assert.IsNotNull(statusResult.Value);
        }
        [Test]
        public async Task GetSimilarMovies_StoresResultsInSession()
        {
            // Arrange
            var testTitle = "Inception";
            var expectedRecommendations = new List<MovieRecommendation> // Changed to MovieRecommendation
            {
                new MovieRecommendation { Title = "The Matrix", Year = 1999 }
            };

            _mockOpenAIService.Setup(s => s.GetSimilarMoviesAsync(testTitle))
                .ReturnsAsync(expectedRecommendations);

            // Setup mock HttpContext
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] sessionData = null;
            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionData = value);
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _homeController.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            // Act
            await _homeController.GetSimilarMovies(testTitle);

            // Assert
            mockSession.Verify(s => s.Set("LastRecommendations", It.IsAny<byte[]>()), Times.Once);
            mockSession.Verify(s => s.Set("LastRecommendationTitle", It.IsAny<byte[]>()), Times.Once);
        }
    }
}