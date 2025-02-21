using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.Controllers;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;

namespace MME_Tests
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IMovieService> _mockMovieService;
        private Mock<ISubscriptionService> _mockSubscriptionService;
        private HomeController _controller;
        private Mock<UserManager<User>> _mockUserManager;

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _mockMovieService = new Mock<IMovieService>();
            _mockSubscriptionService = new Mock<ISubscriptionService>();
            _controller = new HomeController(_mockMovieService.Object, _mockSubscriptionService.Object, _mockUserManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose(); // Ensuring proper cleanup
        }

        [Test]
        public async Task SearchMovies_ValidQuery_ReturnsJsonResult()
        {
            // Arrange
            var query = "Inception";
            var movies = new List<Movie> { new Movie { Title = "Inception", ReleaseYear = 2010 } };
            _mockMovieService.Setup(s => s.SearchMoviesAsync(query)).ReturnsAsync(movies);

            // Act
            var result = await _controller.SearchMovies(query) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);
            Assert.That(result.Value, Is.EqualTo(movies));
        }

        [Test]
        public async Task SearchMovies_EmptyQuery_ReturnsEmptyJsonObject()
        {
            var query = "";

            var result = await _controller.SearchMovies(query) as JsonResult;

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

            var result = await _controller.SearchMovies(query) as JsonResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonResult.Count == 0);
        }
    }
}