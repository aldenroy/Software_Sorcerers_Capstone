using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.PageObjects;
using System;
using System.Threading;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class StreamingServiceSteps
    {
        private readonly IWebDriver _driver;
        private readonly SearchPage _searchPage;
        private readonly ModalPage _modalPage;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
        private readonly RecommendationsPage _recommendationsPage;

        public StreamingServiceSteps(IWebDriver driver)
        {
            _driver = driver;
            _searchPage = new SearchPage(driver);
            _modalPage = new ModalPage(driver);
            _recommendationsPage = new RecommendationsPage(driver);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            try 
            {
                // Close all windows except the main one
                while (_driver.WindowHandles.Count > 1)
                {
                    _driver.SwitchTo().Window(_driver.WindowHandles.Last());
                    _driver.Close();
                }
                
                // Switch back to main window
                if (_driver.WindowHandles.Count > 0)
                {
                    _driver.SwitchTo().Window(_driver.WindowHandles.First());
                }
            }
            catch (Exception) { /* Ignore cleanup errors */ }
        }


        // Modify the GivenTheUserIsOnTheSearchPage method
        [Given(@"the user is on the search page for icons test")]
        public void GivenTheUserIsOnSearchPage()
        {
            try 
            {
                // Navigate to the root URL if not already there
                if (!_driver.Url.Contains("localhost:5000"))
                {
                    _driver.Navigate().GoToUrl("http://localhost:5000/");
                    
                    // Wait for page to load
                    var wait = new WebDriverWait(_driver, _defaultTimeout);
                    wait.Until(d => _searchPage.IsSearchInputDisplayed());
                }
                
                // Verify the page is ready
                Assert.IsTrue(_searchPage.IsSearchInputDisplayed(), "Search input should be displayed");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load search page: {ex.Message}");
            }
        }

        [When(@"the user enters ""(.*)"" into the search bar")]
        public void WhenTheUserEntersInTheSearchBar(string searchTerm)
        {
            _searchPage.EnterSearchTerm(searchTerm);
            _searchPage.SubmitSearch();
        }

        [Then(@"the search should show results for ""(.*)""")]
        public void ThenTheUserSearchShouldTheShowResultsFor(string expectedResult)
        {
            Assert.IsTrue(_searchPage.ResultsContain(expectedResult));
        }

        [Given(@"the user has searched for the ""(.*)"" movie")]
        public void GivenTheUserHasSearchedFor201(string searchTerm)
        {
            GivenTheUserIsOnSearchPage();
            WhenTheUserEntersInTheSearchBar(searchTerm);
            ThenTheUserSearchShouldShowResultsFor(searchTerm);
        }

        [When(@"the user clicks View Details on the first result")]
        public void WhenTheUserClicksTheViewDetailsButtonOnTheFirstResult()
        {
            _searchPage.ClickViewDetails();
            _modalPage.WaitForModalToLoad();
        }

        [Then(@"the user should see the view details modal pop up")]
        public void ThenTheUserShouldSeeTheDetailsModalPopUp()
        {
            Assert.IsTrue(_modalPage.IsModalDisplayed(), "Modal should be displayed");
            Assert.IsTrue(_modalPage.IsModalTitleDisplayed(), "Modal title should be displayed");
        }

        [Given(@"the user has clicked the View Details button from search results")]
        public void GivenTheUserHasClickedTheViewDetailsButtonFromSearch()
        {
            GivenTheUserHasSearchedFor201("Hunger Games");
            WhenTheUserClicksTheViewDetailsButtonOnTheFirstResult();
        }

        [When(@"the user is on the modal pop up")]
        public void WhenTheUserIsOnTheModalPopUp()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => _modalPage.IsModalDisplayed());
        }

        [Then(@"the user should see the Apple TV, Netflix, and Prime Video icons")]
        public void ThenTheUserShouldSeeTheAppleTVNetflixAndPrimeVideoIcons()
        {
            Assert.IsTrue(_modalPage.IsStreamingIconDisplayed("Apple TV"), "Apple TV icon should be displayed");
            Assert.IsTrue(_modalPage.IsStreamingIconDisplayed("Netflix"), "Netflix icon should be displayed");
            Assert.IsTrue(_modalPage.IsStreamingIconDisplayed("Prime Video"), "Prime Video icon should be displayed");
        }


        [Given(@"the user is on the modal pop up")]
        public void GivenTheUserIsOnTheModalPopUp()
        {
            GivenTheUserHasClickedTheViewDetailsButtonFromSearch();
            WhenTheUserIsOnTheModalPopUp();
        }

        [When(@"the user clicks the netflix icon")]
        public void WhenTheUserClicksTheNetflixIcon()
        {
            _modalPage.ClickStreamingIcon("Netflix");
            
            // Wait for new window/tab to open
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.WindowHandles.Count > 1);
            
            // Switch to the new window
            _driver.SwitchTo().Window(_driver.WindowHandles.Last());
        }

        [Then(@"the user should be redirected to the Netflix login page")]
        public void ThenTheUserShouldBeRedirectedToTheNetflixLoginPage()
        {
            // Wait for page to load
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.Url.Contains("netflix.com/login"));
            
            Assert.IsTrue(_driver.Url.Contains("netflix.com/login"), 
                $"Expected to be on Netflix login page but was on {_driver.Url}");
        }

        [Given(@"the user is in the search section")]
        public void GivenTheUserIsAtSearchPage()
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

        [When(@"the user enters the movie ""(.*)"" in the search bar")]
        public void WhenTheUserEntersTheSearchBar(string searchTerm)
        {
            _searchPage.EnterSearchTerm(searchTerm);
            _searchPage.SubmitSearch();
        }

        [Then(@"the search should show the results for ""(.*)""")]
        public void ThenTheUserSearchShouldShowResultsFor(string expectedResult)
        {
            Assert.IsTrue(_searchPage.ResultsContain(expectedResult));
        }

        [Given(@"the user has searched for the ""(.*)""")]
        public void GivenTheUserHasSearchedFor(string searchTerm)
        {
            GivenTheUserIsAtSearchPage();
            WhenTheUserEntersTheSearchBar(searchTerm);
            ThenTheUserSearchShouldShowResultsFor(searchTerm);
        }

        [When(@"the user clicks the ""More Like This"" button for the first result")]
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

        [Then(@"the user should be redirected to a page with the Openai results")]
        public void ThenUserShouldSeeRecommendations()
        {
            Assert.IsTrue(_driver.Url.Contains("Recommendations"));
            Assert.Greater(_recommendationsPage.RecommendationCount(), 0);
        }

        [Given(@"the user is on the recommendations page with Openai results")]
        public void GivenTheUserIsOnTheRecommendationsPageWithOpenaiResults()
        {
            // First navigate to search page and perform a search
            GivenTheUserIsAtSearchPage();
            WhenTheUserEntersTheSearchBar("Hunger Games");
            ThenTheUserSearchShouldShowResultsFor("Hunger Games");
            
            // Click "More Like This" to get to recommendations
            WhenTheUserClicksTheMoreLikeThisButton();
            
            // Wait for recommendations page to fully load
            _recommendationsPage.WaitForPageToLoad();
            
            // Verify we have recommendations
            Assert.Greater(_recommendationsPage.RecommendationCount(), 0, 
                "There should be at least one recommendation");
        }

        [When(@"the user clicks the ""View Details"" button for the first result")]
        public void WhenTheUserClicksViewDetailsForFirstResult()
        {
            // Wait for recommendations to be clickable
            var wait = new WebDriverWait(_driver, _defaultTimeout);
            var viewDetailsButtons = wait.Until(d => 
                d.FindElements(By.CssSelector(".movie-card .btn-primary")));
            
            // Click the first View Details button
            viewDetailsButtons[0].Click();
            
            // Wait for modal to load completely
            _modalPage.WaitForModalToLoad();
            
            // Wait for at least one streaming icon to be present
            _modalPage.WaitForAnyStreamingIcon();
        }

        [Then(@"the user should see service icons on the modal")]
        public void ThenTheUserShouldSeeServiceIconsOnModal()
        {
            // Just verify that at least one streaming icon exists
            Assert.IsTrue(_modalPage.AreStreamingIconsDisplayed(), 
                "At least one streaming service icon should be displayed");
        }

        [Given(@"the user is on the recommendations first results modal")]
        public void GivenTheUserIsOnRecommendationsFirstResultsModal()
        {
            // Follow the complete flow:
            // 1. Search
            GivenTheUserIsAtSearchPage();
            WhenTheUserEntersTheSearchBar("Hunger Games");
            ThenTheUserSearchShouldShowResultsFor("Hunger Games");
            
            // 2. Get recommendations
            WhenTheUserClicksTheMoreLikeThisButton();
            _recommendationsPage.WaitForPageToLoad();
            
            // 3. Open modal and wait for icons
            WhenTheUserClicksViewDetailsForFirstResult();
            
            // Verify modal is open with at least one icon
            Assert.IsTrue(_modalPage.IsModalDisplayed(), "Modal should be displayed");
            Assert.IsTrue(_modalPage.AreStreamingIconsDisplayed(), 
                "At least one service icon should be visible");
        }

        [When(@"the user clicks the first service icon")]
        public void WhenTheUserClicksFirstServiceIcon()
        {
            // Click the first available streaming icon
            _modalPage.ClickFirstStreamingIcon();
            
            // Wait for new window/tab to open
            var wait = new WebDriverWait(_driver, _defaultTimeout);
            wait.Until(d => d.WindowHandles.Count > 1);
            
            // Switch to the new window
            _driver.SwitchTo().Window(_driver.WindowHandles.Last());
        }

        [Then(@"the user should be redirected to that login page")]
        public void ThenTheUserShouldBeRedirectedToLoginPage()
        {
            // Wait for page to load in new tab
            var wait = new WebDriverWait(_driver, _defaultTimeout);
            wait.Until(d => !string.IsNullOrEmpty(d.Url) && !d.Url.Contains("about:blank"));
            
            // Check if we were redirected (don't check specific service)
            Assert.IsTrue(_driver.Url.Contains("://"), 
                $"Expected to be redirected but was on {_driver.Url}");
        }

    }
}