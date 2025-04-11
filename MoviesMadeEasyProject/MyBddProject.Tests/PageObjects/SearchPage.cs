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
        public bool ResultsContain(string expectedText)
        {
            try 
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.FindElement(By.Id("results")).Displayed);
                
                // First check if any movie cards exist
                var cards = _driver.FindElements(By.CssSelector(".movie-card"));
                if (cards.Count == 0) return false;
                
                // Then check if any card contains the expected text
                return cards.Any(card => card.Text.Contains(expectedText));
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}