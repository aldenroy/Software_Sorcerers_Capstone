using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.PageObjects
{
    public class ModalPage
    {
        private readonly IWebDriver _driver;

        public ModalPage(IWebDriver driver)
        {
            _driver = driver;
        }

        public bool IsModalDisplayed()
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                return wait.Until(d => 
                    d.FindElement(By.CssSelector(".modal.show")).Displayed);
            }
            catch
            {
                return false;
            }
        }

        public bool IsModalTitleDisplayed()
        {
            try
            {
                return _driver.FindElement(By.Id("modalTitle")).Displayed;
            }
            catch
            {
                return false;
            }
        }

        public bool AreStreamingIconsDisplayed()
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var icons = wait.Until(d => 
                    d.FindElements(By.CssSelector(".streaming-icon")));
                return icons.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public void ClickStreamingIcon(string serviceName)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var icon = wait.Until(d => 
                d.FindElement(By.CssSelector($".streaming-icon[alt='{serviceName}']")));
            icon.Click();
        }

        public void ClickFirstStreamingIcon()
        {
            var icon = _driver.FindElement(By.CssSelector(".streaming-icon"));
            icon.Click();
        }

        public void WaitForModalToLoad()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => 
                d.FindElement(By.CssSelector(".modal.show")).Displayed && 
                d.FindElement(By.Id("modalTitle")).Displayed);
        }

        // Add this method to wait for streaming icons
        public void WaitForStreamingIcons()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(d => 
                d.FindElements(By.CssSelector(".streaming-icon")).Count >= 3);
        }

        // Modify IsStreamingIconDisplayed
        public bool IsStreamingIconDisplayed(string serviceName)
        {
            try
            {
                var icons = _driver.FindElements(By.CssSelector(".streaming-icon"));
                return icons.Any(icon => 
                    icon.GetAttribute("alt")?.Contains(serviceName, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            catch
            {
                return false;
            }
        }

        public void ClickMoreLikeThisButton()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            var button = wait.Until(d => d.FindElement(By.CssSelector(".btn-more-like-this")));
            button.Click();
        }

        public bool AreRecommendationsDisplayed()
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
                return wait.Until(d => d.FindElements(By.CssSelector(".recommendation-item")).Count > 0);
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetRecommendationTitles()
        {
            return _driver.FindElements(By.CssSelector(".recommendation-item h5"))
                        .Select(e => e.Text)
                        .ToList();
        }
    }
}