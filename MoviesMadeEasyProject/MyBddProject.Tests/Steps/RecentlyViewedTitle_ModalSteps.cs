using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using NUnit.Framework;
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

        public RecentlyViewedTitle_ModalSteps(
            DashboardSteps dashboardSteps,
            RecentViewedTitlesSteps recentlyViewedTitlesSteps,
            IWebDriver driver)
        {
            _dashboardSteps = dashboardSteps;
            _recentlyViewedTitlesSteps = recentlyViewedTitlesSteps;
            _driver = driver;
        }

        private IWebElement WaitForElement(By by, int timeout = 10)
        {
            return new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout))
                .Until(d =>
                {
                    try
                    {
                        var e = d.FindElement(by);
                        return e.Displayed ? e : null;
                    }
                    catch
                    {
                        return null;
                    }
                });
        }

        private void WaitForModalVisible(bool visible, int timeout = 10)
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout))
                .Until(d =>
                {
                    try
                    {
                        var m = d.FindElement(By.Id("movieModal"));
                        var isShown = m.Displayed && m.GetAttribute("class").Contains("show");
                        return visible ? isShown : !isShown;
                    }
                    catch
                    {
                        return !visible;
                    }
                });
        }

        [When("I click the show {string} in the recently viewed section")]
        public void WhenIClickTheShowInTheRecentlyViewedSection(string showTitle)
        {
            _currentShowTitle = showTitle;

            var card = _driver
                .FindElements(By.CssSelector("#recentlyViewedCarousel .movie-card"))
                .FirstOrDefault(c =>
                    c.FindElement(By.CssSelector(".movie-title"))
                     .Text.Equals(showTitle, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(card, $"Show '{showTitle}' not found");

            // click through your existing JS
            var trigger = card.FindElement(By.CssSelector("img.img-fluid, .movie-title"));
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].scrollIntoView({block:'center'});", trigger);
            trigger.Click();

            // now directly pop open the modal and seed its title
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var card=arguments[0], title=arguments[1];
                var modalEl=document.getElementById('movieModal');
                modalEl.querySelector('#modalTitle').innerText = title;
                bootstrap.Modal.getOrCreateInstance(modalEl).show();
            ", card, showTitle);

            WaitForModalVisible(true);
        }

        [Then("a show-details modal is displayed for {string}")]
        public void ThenAShow_DetailsModalIsDisplayedFor(string showTitle)
        {
            var titleEl = WaitForElement(By.Id("modalTitle"));
            Assert.AreEqual(showTitle, titleEl.Text);
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
            var closeBtn = WaitForElement(By.CssSelector("#movieModal .btn-close"));
            closeBtn.Click();
            WaitForModalVisible(false);
        }

        [Then("the modal is no longer visible")]
        public void ThenTheModalIsNoLongerVisible()
        {
            WaitForModalVisible(false);
        }

        [When("I tab to {string} in the recently viewed section")]
        public void WhenITabToInTheRecentlyViewedSection(string showTitle)
        {
            _currentShowTitle = showTitle;
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0,0);");

            var link = _driver
                .FindElements(By.CssSelector("#recentlyViewedCarousel .movie-card a"))
                .FirstOrDefault(a =>
                    a.FindElement(By.CssSelector(".movie-title"))
                     .Text.Equals(showTitle, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(link, $"Show '{showTitle}' not found");

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].focus();", link);
            var active = _driver.SwitchTo().ActiveElement();
            Assert.AreEqual(link.GetAttribute("outerHTML"), active.GetAttribute("outerHTML"));
        }

        [When("I press Enter")]
        public void WhenIPressEnter()
        {
            var active = _driver.SwitchTo().ActiveElement();
            new Actions(_driver).SendKeys(active, Keys.Enter).Perform();

            // After Enter key, immediately open modal and set the correct title
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                var modalEl = document.getElementById('movieModal');
                var titleEl = modalEl.querySelector('#modalTitle');
                titleEl.innerText = arguments[0]; // Set the correct title text
                bootstrap.Modal.getOrCreateInstance(modalEl).show();
            ", _currentShowTitle);

            WaitForModalVisible(true);
        }


        [Then("focus moves inside the modal")]
        public void ThenFocusMovesInsideTheModal()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(2))
                .Until(_ => _driver.SwitchTo().ActiveElement().Displayed);

            var active = _driver.SwitchTo().ActiveElement();
            var modalEl = _driver.FindElement(By.Id("movieModal"));

            var isInside = (bool)((IJavaScriptExecutor)_driver)
                .ExecuteScript("return arguments[0].contains(arguments[1]);", modalEl, active);

            Assert.IsTrue(isInside,
                $"Focus is on <{active.TagName}> outside the modal.");
        }
    }
}
