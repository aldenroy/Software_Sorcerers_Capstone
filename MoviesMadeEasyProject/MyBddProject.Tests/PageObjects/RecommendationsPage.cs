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

            // Wait for loading to complete
            wait.Until(d => 
                d.FindElement(By.Id("loadingSpinner")).GetCssValue("display") == "none");

            // Wait for content to appear
            wait.Until(d => 
                d.FindElements(By.CssSelector("#recommendationsContainer .list-group-item")).Count > 0 ||
                d.FindElements(By.CssSelector(".recommendations-error")).Count > 0);
        }

        public int RecommendationCount()
        {
            return _driver.FindElements(By.CssSelector("#recommendationsContainer .list-group-item")).Count;
        }

        public void ClickBackToSearch()
        {
            var backButton = _wait.Until(d => 
                d.FindElement(By.CssSelector(".back-to-search .btn-primary")));
            backButton.Click();
            
            // Wait to leave recommendations page
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