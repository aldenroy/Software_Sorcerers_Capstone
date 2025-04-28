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

        public void ClickStreamingIcon(string serviceName)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var icon = wait.Until(d => 
                d.FindElement(By.CssSelector($".streaming-icon[alt='{serviceName}']")));
            icon.Click();
        }

        public void WaitForModalToLoad()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            
            // Wait for modal to be visible
            wait.Until(d => 
                d.FindElement(By.CssSelector(".modal.show")).Displayed);
            
            // Wait for loading to complete (wait for loading message to disappear)
            wait.Until(d => 
                !d.FindElement(By.Id("modalStreaming")).Text.Contains("Loading"));
        }

        public void WaitForStreamingIcons(int minCount = 3)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            
            // Wait for at least minCount streaming icons to be present and visible
            wait.Until(d => 
            {
                try 
                {
                    var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                    return icons.Count >= minCount && icons.All(icon => icon.Displayed);
                }
                catch
                {
                    return false;
                }
            });
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

        public void WaitForAnyStreamingIcon()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(d => 
            {
                var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                return icons.Count > 0 && icons[0].Displayed;
            });
        }

        public bool AreStreamingIconsDisplayed()
        {
            try
            {
                var icons = _driver.FindElements(By.CssSelector(".streaming-icon"));
                return icons.Count > 0 && icons[0].Displayed;
            }
            catch
            {
                return false;
            }
        }

        public void ClickFirstStreamingIcon()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            var icon = wait.Until(d => 
            {
                var icons = d.FindElements(By.CssSelector(".streaming-icon"));
                return icons.Count > 0 ? icons[0] : null;
            });
            icon.Click();
        }
    }
}