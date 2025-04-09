using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Reqnroll;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.Tests.PageObjects;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class DashboardSteps
    {
        private IWebDriver _driver;

        [BeforeScenario]
        public void SetUp()
        {
            _driver = new ChromeDriver();
        }

        [AfterScenario]
        public void TearDown()
        {
            _driver.Quit();
        }

        [Given(@"I am logged in on the dashboard page")]
        public void GivenIAmLoggedInOnTheDashboardPage()
        {
            var loginPage = new LoginPageTestSetup(_driver);
            loginPage.GoTo();
            loginPage.Login("test@mail.com", "Ab+1234");
        }

        [When("the page loads")]
        public void WhenThePageLoads()
        {
        }

        [When("I navigate to the navbar")]
        public void WhenINavigateToTheNavbar()
        {
        }

        [Then("I should see a {string} link in the navbar")]
        public void ThenIShouldSeeALinkInTheNavbar(string dashboard)
        {
            var navLinks = _driver.FindElements(By.CssSelector("#navbar-primary .nav-link"));
            var linkFound = navLinks.Any(link => link.Text.Trim().Equals(dashboard, StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(linkFound, $"Expected to find a link with text '{dashboard}' in the navbar, but did not.");
        }

        [Given(@"I navigate the ""(.*)"" page")]
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
            var targetLink = navLinks.FirstOrDefault(link => link.Text.Trim().Equals(linkText, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(targetLink, $"Could not find a navbar link with text '{linkText}'");

            targetLink.Click();
        }

        [Then("I should be redirected to my dashboard page")]
        public void ThenIShouldBeRedirectedToMyDashboardPage()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(driver => driver.Url.Contains("/User/Dashboard"));

            Assert.IsTrue(_driver.Url.Contains("/User/Dashboard"), $"Expected dashboard page but got {_driver.Url}");
        }

        [When(@"I tab through the navbar until I reach the ""(.*)"" link")]
        public void WhenITabThroughTheNavbarUntilIReachTheLink(string linkText)
        {
            var maxTabs = 20;
            bool found = false;

            for (int i = 0; i < maxTabs; i++)
            {
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

        [Then(@"the ""(.*)"" link should include a clear, descriptive label that lets my screen reader announce its purpose\.")]
        public void ThenTheLinkShouldIncludeADescriptiveLabel(string linkText)
        {
            var link = _driver.FindElements(By.CssSelector("#navbar-primary .nav-link"))
                              .FirstOrDefault(e => e.Text.Trim().Equals(linkText, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(link, $"Could not find '{linkText}' in the navbar.");

            var ariaLabel = link.GetAttribute("aria-label");

            Assert.IsFalse(string.IsNullOrWhiteSpace(ariaLabel), "'aria-label' is missing or empty.");
            Assert.IsTrue(ariaLabel.Contains("dashboard", StringComparison.OrdinalIgnoreCase), $"aria-label should describe purpose. Found: '{ariaLabel}'");
        }

        [When(@"I click on a subscription bubble for ""(.*)""")]
        public void WhenIClickOnASubscriptionBubbleFor(string serviceName)
        {
            var link = _driver.FindElements(By.CssSelector(".subscription-link"))
                .FirstOrDefault(el => el.GetAttribute("aria-label")?.Contains(serviceName, StringComparison.OrdinalIgnoreCase) == true);

            Assert.IsNotNull(link, $"Could not find a subscription link for '{serviceName}'.");

            var originalWindow = _driver.CurrentWindowHandle;
            var existingWindows = _driver.WindowHandles.ToList();

            link.Click();

            new WebDriverWait(_driver, TimeSpan.FromSeconds(5)).Until(
                driver => driver.WindowHandles.Count > existingWindows.Count
            );

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

    }
}
