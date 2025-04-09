using OpenQA.Selenium;

namespace MyBddProject.PageObjects
{
    public class RecommendationsPage
    {
        private readonly IWebDriver _driver;

        public RecommendationsPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public bool IsRecommendationFor(string originalTitle)
        {
            var header = _driver.FindElement(By.CssSelector("#recommendationsContainer h3"));
            return header.Text.Contains(originalTitle);
        }

        public int RecommendationCount()
        {
            var items = _driver.FindElements(By.CssSelector("#recommendationsContainer .list-group-item"));
            return items.Count;
        }
    }
}