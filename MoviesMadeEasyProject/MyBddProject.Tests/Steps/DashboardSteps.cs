using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using MyBddProject.Tests.PageObjects;
using Reqnroll;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class DashboardSteps
    {
        private IWebDriver _driver;
    public DashboardSteps(IWebDriver driver)
    {
        _driver = driver;
    }

    // Scenario: Display Dashboard Link for Authenticated User

    [Given(@"I am logged in on the dashboard page")]
        public void GivenIAmLoggedInOnTheDashboardPage()
        {
            var loginPage = new LoginPageTestSetup(_driver);
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");
            loginPage.Login("testuser@example.com", "Ab+1234");
        }

        [When(@"the page loads")]
        public void WhenThePageLoads()
        {
        }

        [Then(@"I should see a ""(.*)"" link in the navbar")]
        public void ThenIShouldSeeALinkInTheNavbar(string dashboard)
        {
            var navLinks = _driver.FindElements(By.CssSelector("#navbar-primary .nav-link"));
            var linkFound = navLinks.Any(link => link.Text.Trim().Equals(dashboard, StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(linkFound, $"Expected to find a link with text '{dashboard}' in the navbar, but did not.");
        }

        // Scenario: Navigate to the Dashboard Page

        [Given(@"I navigate to the {string} page")]
        public void GivenINavigateThePage(string pageName)
        {
            string url = pageName.ToLower() switch
            {
                "home" => "http://localhost:5000/",
                _ => throw new ArgumentException($"Unknown page: {pageName}")
            };

            _driver.Navigate().GoToUrl(url);
        }

        [When(@"I click the ""(.*)"" link on the navbar")]
        public void WhenIClickTheLinkOnTheNavbar(string linkText)
        {
            var navLinks = _driver.FindElements(By.CssSelector("#navbar-primary .nav-link"));
            var targetLink = navLinks.FirstOrDefault(link =>
                link.Text.Trim().Equals(linkText, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(targetLink, $"Could not find a navbar link with text '{linkText}'");

            targetLink.Click();
        }

        [Then(@"I should be redirected to my dashboard page")]
        public void ThenIShouldBeRedirectedToMyDashboardPage()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(driver => driver.Url.Contains("/User/Dashboard"));

            Assert.IsTrue(_driver.Url.Contains("/User/Dashboard"), $"Expected dashboard page but got {_driver.Url}");
        }

        // Scenario: Keyboard Navigation for Dashboard Button

        [When(@"I tab through the navbar until I reach the ""(.*)"" link")]
        public void WhenITabThroughTheNavbarUntilIReachTheLink(string linkText)
        {
            var maxTabs = 20;
            bool found = false;

            for (int i = 0; i < maxTabs; i++)
            {
                System.Threading.Thread.Sleep(100);
                var currentElement = _driver.SwitchTo().ActiveElement();

                if (currentElement.Text.Trim().Equals(linkText, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
                currentElement.SendKeys(Keys.Tab);
            }

            Assert.IsTrue(found, $"Did not find a focusable element with text '{linkText}' using keyboard tabbing.");
        }

        [Then(@"I should be able to focus on and activate the button using the keyboard")]
        public void ThenIShouldBeAbleToFocusOnAndActivateTheButton()
        {
            var activeElement = _driver.SwitchTo().ActiveElement();
            activeElement.SendKeys(Keys.Enter);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(driver => driver.Url.Contains("/User/Dashboard"));

            Assert.IsTrue(_driver.Url.Contains("/User/Dashboard"), $"Expected to be on dashboard page but was on {_driver.Url}");
        }

        // Scenario: Screen Reader Accessibility for Dashboard Link

        [When(@"I navigate to the navbar")]
        public void WhenINavigateToTheNavbar()
        {
        }

        [Then(@"the ""(.*)"" link should include a clear, descriptive label that lets my screen reader announce its purpose\.")]
        public void ThenTheLinkShouldIncludeADescriptiveLabel(string linkText)
        {
            var link = _driver.FindElements(By.CssSelector("#navbar-primary .nav-link"))
                              .FirstOrDefault(e => e.Text.Trim().Equals(linkText, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(link, $"Could not find '{linkText}' in the navbar.");

            var ariaLabel = link.GetAttribute("aria-label");

            Assert.IsFalse(string.IsNullOrWhiteSpace(ariaLabel), "'aria-label' is missing or empty.");
            Assert.IsTrue(ariaLabel.Contains("dashboard", StringComparison.OrdinalIgnoreCase),
                $"aria-label should describe purpose. Found: '{ariaLabel}'");
        }


        // Scenario : Icon Navigation to Subscription Login

        [When(@"I click on a subscription bubble for ""(.*)""")]
        public void WhenIClickOnASubscriptionBubbleFor(string serviceName)
        {
            var link = _driver.FindElements(By.CssSelector(".subscription-link"))
                .FirstOrDefault(el => el.GetAttribute("aria-label")?.Contains(serviceName, StringComparison.OrdinalIgnoreCase) == true);

            Assert.IsNotNull(link, $"Could not find a subscription link for '{serviceName}'.");

            var existingWindows = _driver.WindowHandles.ToList();

            link.Click();

            new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(
                driver => driver.WindowHandles.Count > existingWindows.Count
            );

            var newWindow = _driver.WindowHandles.Except(existingWindows).First();
            _driver.SwitchTo().Window(newWindow);
        }

        [When("I focus on and activate the subscription icon using the keyboard")]
        public void WhenIFocusOnAndActivateTheSubscriptionIconUsingTheKeyboard()
        {
            var activeElement = _driver.SwitchTo().ActiveElement();
            var existingWindows = _driver.WindowHandles.ToList();

            activeElement.SendKeys(Keys.Enter);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            wait.Until(driver => driver.WindowHandles.Count > existingWindows.Count);
            var newWindow = _driver.WindowHandles.Except(existingWindows).First();
            _driver.SwitchTo().Window(newWindow);
        }


        [Then(@"I should be redirected to that services website login page ""(.*)""")]
        public void ThenIShouldBeRedirectedToThatServiceLoginPage(string expectedUrl)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => driver.Url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(
                _driver.Url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase),
                $"Expected to be redirected to '{expectedUrl}', but ended up on '{_driver.Url}'."
            );
        }

        // Scenario: Keyboard Navigation for Subscription Icons
        [When(@"I tab through the subscription icons until I reach the ""(.*)"" icon")]
        public void WhenITabThroughTheSubscriptionIconsUntilIReachTheIcon(string serviceName)
        {
            int maxTabs = 20;
            bool found = false;

            for (int i = 0; i < maxTabs; i++)
            {
                System.Threading.Thread.Sleep(100);
                var activeElement = _driver.SwitchTo().ActiveElement();
                string ariaLabel = activeElement.GetAttribute("aria-label");

                if (!string.IsNullOrEmpty(ariaLabel) &&
                    ariaLabel.IndexOf(serviceName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = true;
                    break;
                }
                activeElement.SendKeys(Keys.Tab);
            }

            Assert.IsTrue(found, $"Failed to focus on the subscription icon for '{serviceName}' using keyboard navigation.");
        }

        // [Then(@"I should be redirected to that services website login page ""(.*)""")]

        // Scenario: Screen Reader Accessibility for Subscription Icons
        [When(@"I navigate to the subscription icons")]
        public void WhenINavigateToTheSubscriptionIcons()
        {
        }

        [Then(@"the ""(.*)"" subscription icon should include a clear, descriptive accessible label")]
        public void ThenTheSubscriptionIconShouldIncludeAClearDescriptiveAccessibleLabel(string serviceName)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var icon = wait.Until(driver =>
            {
                var icons = driver.FindElements(By.CssSelector(".subscription-link"));
                return icons.FirstOrDefault(el =>
                    el.GetAttribute("aria-label")?.IndexOf(serviceName, StringComparison.OrdinalIgnoreCase) >= 0);
            });

            Assert.IsNotNull(icon, $"Could not find a subscription icon for '{serviceName}'.");
            string ariaLabel = icon.GetAttribute("aria-label");
            Assert.IsFalse(string.IsNullOrWhiteSpace(ariaLabel), "aria-label is missing or empty on the subscription icon.");
            Assert.IsTrue(ariaLabel.IndexOf(serviceName, StringComparison.OrdinalIgnoreCase) >= 0,
                $"aria-label '{ariaLabel}' does not clearly identify the service '{serviceName}'.");
        }
    }
}
