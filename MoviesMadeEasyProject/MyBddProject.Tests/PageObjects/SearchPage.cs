using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.PageObjects
{
    public class SearchPage
    {
        private readonly IWebDriver _driver;

        public SearchPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public void EnterSearchTerm(string term)
        {
            var searchInput = _driver.FindElement(By.Id("searchInput"));
            searchInput.Clear();
            searchInput.SendKeys(term);
        }

        public void SubmitSearch()
        {
            _driver.FindElement(By.Id("searchInput")).SendKeys(Keys.Return);
        }

        public void ClickMoreLikeThis(int resultIndex = 0)
        {
            var buttons = _driver.FindElements(By.CssSelector(".btn-outline-secondary"));
            buttons[resultIndex].Click();
        }

        public bool IsSearchInputDisplayed()
        {
            try
            {
                return _driver.FindElement(By.Id("searchInput")).Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool AreResultsDisplayed()
        {
            try
            {
                return _driver.FindElements(By.CssSelector(".movie-card")).Count > 0 || 
                    _driver.FindElements(By.CssSelector(".no-results")).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public void ClickViewDetails(int resultIndex = 0)
        {
            var buttons = _driver.FindElements(By.CssSelector(".movie-card .btn-primary"));
            buttons[resultIndex].Click();
        }

        // Add this method to check if page is loaded
        public bool IsPageLoaded()
        {
            try
            {
                return _driver.FindElement(By.Id("searchInput")).Displayed;
            }
            catch
            {
                return false;
            }
        }

        // Modify ResultsContain method
        public bool ResultsContain(string expectedText)
        {
            try 
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                
                // Wait for results container to be present
                wait.Until(d => 
                    d.FindElements(By.CssSelector(".movie-card, .no-results")).Count > 0);
                
                // Check for no results first
                var noResults = _driver.FindElements(By.CssSelector(".no-results"));
                if (noResults.Count > 0)
                    return false;
                    
                // Then check movie cards
                var cards = _driver.FindElements(By.CssSelector(".movie-card"));
                return cards.Any(card => card.Text.Contains(expectedText));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking results: {ex.Message}");
                return false;
            }
        }
    }
}