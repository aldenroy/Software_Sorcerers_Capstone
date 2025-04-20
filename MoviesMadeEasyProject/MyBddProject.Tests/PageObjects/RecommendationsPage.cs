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
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // Wait for exact URL
            wait.Until(d => d.Url.Equals(
                "http://localhost:5000/Home/Recommendations",
                StringComparison.OrdinalIgnoreCase));

            // Wait for loading spinner to disappear
            wait.Until(d =>
                d.FindElement(By.Id("loadingSpinner")).GetCssValue("display") == "none");

            // Wait until at least one .movie-card is visible in the container
            wait.Until(d =>
                d.FindElements(By.CssSelector("#recommendationsContainer .movie-card")).Count > 0);
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
    }
}