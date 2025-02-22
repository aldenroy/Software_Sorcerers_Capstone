using NUnit.Framework;
using MoviesMadeEasy.DTOs;
using MoviesMadeEasy.Models;
using System.Collections.Generic;
using System.Linq;

namespace Tests_Unit
{
    [TestFixture]
    public class Tests_Dashboard
    {
        private DashboardDTO dashboard;

        [SetUp]
        public void Setup()
        {
            dashboard = new DashboardDTO();
            dashboard.UserName = "Test";
        }

        [Test]
        public void Dashboard_ZeroSubscriptions_ShowsEmptyList()
        {
            // Arrange 
            dashboard.HasSubscriptions = false;
            dashboard.SubList = new List<StreamingService>();

            // Assert
            Assert.That(dashboard.HasSubscriptions, Is.False);
            Assert.That(dashboard.SubList, Is.Empty);
            Assert.That(dashboard.SubList.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dashboard_OneSubscription_ShowsSingleService()
        {
            // Arrange
            dashboard.HasSubscriptions = true;
            dashboard.SubList = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" }
            };

            // Assert
            Assert.That(dashboard.HasSubscriptions, Is.True);
            Assert.That(dashboard.SubList, Is.Not.Empty);
            Assert.That(dashboard.SubList.Count, Is.EqualTo(1));
            Assert.That(dashboard.SubList.First().Name, Is.EqualTo("Netflix"));
        }

        [Test]
        public void Dashboard_MultipleSubscriptions_ShowsAllServices()
        {
            // Arrange
            dashboard.HasSubscriptions = true;
            dashboard.SubList = new List<StreamingService>
            {
                new StreamingService { Name = "Netflix" },
                new StreamingService { Name = "Hulu" },
                new StreamingService { Name = "Disney+" }
            };

            // Assert
            Assert.That(dashboard.HasSubscriptions, Is.True);
            Assert.That(dashboard.SubList.Count, Is.EqualTo(3));
            Assert.That(dashboard.SubList.Select(s => s.Name).Contains("Netflix"));
            Assert.That(dashboard.SubList.Select(s => s.Name).Contains("Hulu"));
            Assert.That(dashboard.SubList.Select(s => s.Name).Contains("Disney+"));
        }
    }
}