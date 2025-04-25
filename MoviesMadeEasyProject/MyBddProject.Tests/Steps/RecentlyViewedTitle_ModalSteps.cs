using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using System;
using NUnit.Framework;
using System.Linq;
using MyNamespace.Steps;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class RecentlyViewedTitle_ModalSteps
    {
        private readonly DashboardSteps _dashboardSteps;
        private readonly RecentViewedTitlesSteps _recentlyViewedTitlesSteps;
        private readonly IWebDriver _driver;
        private string _currentShowTitle;

        public RecentlyViewedTitle_ModalSteps(DashboardSteps dashboardSteps, RecentViewedTitlesSteps recentViewedTitlesSteps, IWebDriver driver)
        {
            _dashboardSteps = dashboardSteps;
            _recentlyViewedTitlesSteps = recentViewedTitlesSteps;
            _driver = driver;
        }

        private IWebElement WaitForElement(By by, int timeoutInSeconds = 10)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.Displayed ? element : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
        }

        private bool IsModalVisible()
        {
            try
            {
                var modal = _driver.FindElement(By.Id("movieModal"));
                return modal.GetAttribute("class").Contains("show");
            }
            catch
            {
                return false;
            }
        }

        [When("I click the show {string} in the recently viewed section")]
        public void WhenIClickTheShowInTheRecentlyViewedSection(string showTitle)
        {
            _currentShowTitle = showTitle;
            var movieCards = _driver.FindElements(By.CssSelector("#recentlyViewedCarousel .movie-card"));

            var targetCard = movieCards.FirstOrDefault(card =>
                card.FindElement(By.CssSelector(".movie-title")).Text.Equals(showTitle, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(targetCard, $"Show '{showTitle}' not found in the recently viewed section");

            var link = targetCard.FindElement(By.TagName("a"));
            link.Click();

            WaitForElement(By.Id("movieModal"));
        }

        [Then("a show-details modal is displayed for {string}")]
        public void ThenAShow_DetailsModalIsDisplayedFor(string showTitle)
        {
            Assert.IsTrue(IsModalVisible(), "The modal should be visible");

            var modalTitle = WaitForElement(By.Id("modalTitle"));
            Assert.AreEqual(showTitle, modalTitle.Text, "Modal title should match the show title");
        }

        [Given("the show-details modal is displayed for {string}")]
        public void GivenTheShow_DetailsModalIsDisplayedFor(string showTitle)
        {
            WhenIClickTheShowInTheRecentlyViewedSection(showTitle);
            ThenAShow_DetailsModalIsDisplayedFor(showTitle);
        }

        [When("I click the modal close button")]
        public void WhenIClickTheModalCloseButton()
        {
            var closeButton = WaitForElement(By.CssSelector("#movieModal .btn-close"));
            closeButton.Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
            wait.Until(driver => !IsModalVisible());
        }

        [Then("the modal is no longer visible")]
        public void ThenTheModalIsNoLongerVisible()
        {
            Assert.IsFalse(IsModalVisible(), "The modal should not be visible");
        }

        [When("I tab to {string} in the recently viewed section")]
        public void WhenITabToInTheRecentlyViewedSection(string showTitle)
        {
            _currentShowTitle = showTitle;

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 0);");

 
            var links = _driver.FindElements(By.CssSelector("#recentlyViewedCarousel .movie-card a"));
            var targetLink = links.FirstOrDefault(link =>
                link.FindElement(By.CssSelector(".movie-title")).Text.Equals(showTitle, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(targetLink, $"Show '{showTitle}' not found in the recently viewed section");

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].focus();", targetLink);

            var activeElement = _driver.SwitchTo().ActiveElement();
            Assert.AreEqual(targetLink.GetAttribute("outerHTML"), activeElement.GetAttribute("outerHTML"),
                $"The link for '{showTitle}' should have focus");
        }

        [When("I press Enter")]
        public void WhenIPressEnter()
        {
            var activeElement = _driver.SwitchTo().ActiveElement();

            new Actions(_driver)
                .SendKeys(activeElement, Keys.Enter)
                .Perform();

            WaitForElement(By.Id("movieModal"));
        }

        [Then("focus moves inside the modal")]
        public void ThenFocusMovesInsideTheModal()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(2))
                .Until(_ => _driver.SwitchTo().ActiveElement().Displayed);

            var activeElement = _driver.SwitchTo().ActiveElement();
            var modalElement = _driver.FindElement(By.Id("movieModal"));

            bool isFocusInsideModal = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return arguments[0].contains(arguments[1]);",
                               modalElement, activeElement);

            Assert.IsTrue(isFocusInsideModal,
                $"Focus is on <{activeElement.TagName}> outside the modal.");
        }

    }
}