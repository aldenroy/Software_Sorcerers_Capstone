using NUnit.Framework;
using MoviesMadeEasy.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc; 

namespace MME_Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public void HomeController_Index_ReturnsViewResult()
        {
            // Arrange: Create a real logger
            ILogger<HomeController> logger = new LoggerFactory().CreateLogger<HomeController>();

            // Act: Create an instance of HomeController and call Index()
            var controller = new HomeController(logger);
            var result = controller.Index();

            // Assert: Ensure Index() returns a ViewResult (proves real interaction)
            Assert.IsInstanceOf<ViewResult>(result);
        }
    }
}