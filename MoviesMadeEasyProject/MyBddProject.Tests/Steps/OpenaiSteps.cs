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

        // Add these methods to your existing OpenaiSteps class

        [Given(@"the user has clicked the More Like This button")]
        public void GivenTheUserHasClickedTheMoreLikeThisButton()
        {
            // This step is already covered by previous steps
            GivenTheUserHasSearchedFor("Hunger Games");
            WhenTheUserClicksTheMoreLikeThisButton();
            ThenUserShouldSeeRecommendations();
        }

        [When(@"the user is on the recommendations page")]
        public void WhenTheUserIsOnTheRecommendationsPage()
        {
            // Wait for recommendations page to load
            _recommendationsPage.WaitForPageToLoad();
            
            // Verify we're on the recommendations page
            Assert.IsTrue(_driver.Url.Contains("Recommendations"));
        }

        [Then(@"the user should see five suggested results")]
        public void ThenTheUserShouldSeeFiveSuggestedResults()
        {
            var recommendationCount = _recommendationsPage.RecommendationCount();
            Assert.AreEqual(5, recommendationCount, 
                $"Expected 5 recommendations but found {recommendationCount}");
        }

        [Given(@"the user is on the recommendations page")]
        public void GivenTheUserIsOnTheRecommendationsPage()
        {
            // This combines previous steps to get to recommendations page
            GivenTheUserHasClickedTheMoreLikeThisButton();
            WhenTheUserIsOnTheRecommendationsPage();
        }

        [When(@"the user clicks the Back to search button")]
        public void WhenTheUserClicksTheBackToSearchButton()
        {
            _recommendationsPage.ClickBackToSearch();
        }

        [Then(@"the user should be redirected back to the search page")]
        public void ThenTheUserShouldBeRedirectedBackToTheSearchPage()
        {
            // Wait for URL to change back to search page
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(d => !d.Url.Contains("Recommendations"));
            
            // Verify we're back on search page
            Assert.IsTrue(_driver.Url.EndsWith("/") || _driver.Url.Contains("Home/Index"));
            
            // Verify search page elements are present
            Assert.IsTrue(_searchPage.IsSearchInputDisplayed());
        }
    }
}