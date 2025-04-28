using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace MyBddProject.PageObjects
{
    public class RecommendationsPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public RecommendationsPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        public void WaitForPageToLoad()
        {
            // Wait for URL to contain recommendations (more flexible than exact match)
            _wait.Until(d => d.Url.Contains("Recommendations", StringComparison.OrdinalIgnoreCase));

            // Wait for loading spinner to disappear
            _wait.Until(d => 
            {
                try 
                {
                    var spinner = d.FindElement(By.Id("loadingSpinner"));
                    return spinner == null || spinner.GetCssValue("display") == "none";
                }
                catch (NoSuchElementException)
                {
                    return true;
                }
            });

            // Wait for at least 3 movie cards to be present and visible
            _wait.Until(d => 
            {
                var cards = d.FindElements(By.CssSelector("#recommendationsContainer .movie-card"));
                return cards.Count >= 3 && cards.All(c => c.Displayed);
            });
        }


        public int RecommendationCount()
        {
            return _driver.FindElements(By.CssSelector("#recommendationsContainer .movie-card")).Count;
        }

        public void ClickBackToSearch()
        {
            // Wait for the spinner to disappear first
            _wait.Until(d =>
                d.FindElement(By.Id("loadingSpinner")).GetCssValue("display") == "none");

            var backButton = _wait.Until(d =>
                d.FindElement(By.CssSelector(".back-to-search .btn-primary")));

            // Scroll into view and wait until clickable
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", backButton);
            _wait.Until(d => backButton.Displayed && backButton.Enabled);

            try
            {
                backButton.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // Try again after a brief wait if intercepted
                Thread.Sleep(500);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", backButton);
            }

            // Wait for navigation to happen
            _wait.Until(d => !d.Url.Contains("Recommendations"));
        }



        public bool IsRecommendationFor(string originalTitle)
        {
            try
            {
                var header = _driver.FindElement(By.CssSelector("#recommendationsContainer h3"));
                return header.Text.Contains(originalTitle);
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
    }
}