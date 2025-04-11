using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI; // Add this line
using NUnit.Framework;
using MyBddProject.PageObjects;
using System;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class OpenaiSteps
    {
        private readonly IWebDriver _driver;
        private readonly SearchPage _searchPage;
        private readonly RecommendationsPage _recommendationsPage;

        public OpenaiSteps(IWebDriver driver)
        {
            _driver = driver;
            _searchPage = new SearchPage(driver);
            _recommendationsPage = new RecommendationsPage(driver);
        }

        [Given(@"the user is on the search page")]
        public void GivenTheUserIsOnTheSearchPage()
        {
            // Navigate to the root URL if not already there
            if (!_driver.Url.Contains("localhost:5000"))
            {
                _driver.Navigate().GoToUrl("http://localhost:5000/");
            }
            
            // Optionally, you could verify the search page elements are present
            Assert.IsTrue(_driver.PageSource.Contains("Movie Search")); // Verify the page title
            Assert.IsTrue(_driver.FindElement(By.Id("searchInput")).Displayed); // Verify search input is present
        }

        [When(@"the user enters ""(.*)"" in the search bar")]
        public void WhenTheUserEntersInTheSearchBar(string searchTerm)
        {
            _searchPage.EnterSearchTerm(searchTerm);
            _searchPage.SubmitSearch();
        }

        [Then(@"the user search should show results for ""(.*)""")]
        public void ThenTheUserSearchShouldShowResultsFor(string expectedResult)
        {
            Assert.IsTrue(_searchPage.ResultsContain(expectedResult));
        }

        [Given(@"the user has searched for ""(.*)""")]
        public void GivenTheUserHasSearchedFor(string searchTerm)
        {
            GivenTheUserIsOnTheSearchPage();
            WhenTheUserEntersInTheSearchBar(searchTerm);
            ThenTheUserSearchShouldShowResultsFor(searchTerm);
        }

        [When(@"the user clicks the ""More Like This"" button on the first result")]
        public void WhenTheUserClicksTheMoreLikeThisButton()
        {
            // Wait for results to load first
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => d.FindElements(By.CssSelector(".movie-card")).Count > 0);
            
            _searchPage.ClickMoreLikeThis();
            
            // Wait for recommendations page to load
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => d.Url.Contains("Recommendations"));
        }

        [Then(@"the user should be redirected to a new page with the Openai results")]
        public void ThenUserShouldSeeRecommendations()
        {
            Assert.IsTrue(_driver.Url.Contains("Recommendations"));
            Assert.Greater(_recommendationsPage.RecommendationCount(), 0);
        }
    }
}