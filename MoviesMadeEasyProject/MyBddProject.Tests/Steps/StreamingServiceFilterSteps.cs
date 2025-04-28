using NUnit.Framework;
using Reqnroll;
using Reqnroll.BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class StreamingServiceFilterSteps
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const string MovieCardCss = ".movie-card";
        private const string StreamingFiltersCss = "#streaming-filters input[type='checkbox']";

        public StreamingServiceFilterSteps(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Given("the user is on the content browsing page")]
        public void GivenTheUserIsOnTheContentBrowsingPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000");
            Assert.IsTrue(_driver.FindElement(By.Id("searchInput")).Displayed,
                          "Search input should be visible");
        }

        [Given("the list of available streaming services is displayed")]
        public void GivenTheListOfAvailableStreamingServicesIsDisplayed()
        {
            var input = _driver.FindElement(By.Id("searchInput"));
            input.Clear();
            input.SendKeys("a");
            input.SendKeys(Keys.Enter);

            _wait.Until(d => d.FindElements(By.CssSelector(StreamingFiltersCss)).Any());
            var boxes = _driver.FindElements(By.CssSelector(StreamingFiltersCss));
            Assert.IsTrue(boxes.Any(), "Streaming filters not displayed");
        }

        [Given("the user has one or more streaming service filters applied")]
        public void GivenTheUserHasOneOrMoreStreamingServiceFiltersApplied()
        {
            GivenTheListOfAvailableStreamingServicesIsDisplayed();
            var first = _driver.FindElements(By.CssSelector(StreamingFiltersCss)).First();
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].click(); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                first);
        }

        [When("the user selects a streaming service filter")]
        public void WhenTheUserSelectsAStreamingServiceFilter()
        {
            var first = _wait.Until(d => d.FindElements(By.CssSelector(StreamingFiltersCss)).FirstOrDefault());
            Assert.IsNotNull(first, "No streaming filter checkbox found");
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].scrollIntoView(true); arguments[0].click(); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                first);

            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss)).Any(c => c.Displayed));
        }

        [When("the user selects a specific streaming service \"(.*)\"")]
        public void WhenTheUserSelectsASpecificStreamingService(string service)
        {
            var js = (IJavaScriptExecutor)_driver;
            var script =
                "var cb = document.querySelector('#streaming-filters input[value=\\\"" + service + "\\\"]');" +
                "if(cb){cb.checked=true;cb.dispatchEvent(new Event('change', { bubbles: true }));}";
            js.ExecuteScript(script);

            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss))
                .Where(card => card.Displayed)
                .All(card => card.GetAttribute("data-streaming")
                    .Split(',').Select(s => s.Trim())
                    .Contains(service)));
        }

        [When("the user selects streaming services \"(.*)\" and \"(.*)\"")]
        public void WhenTheUserSelectsStreamingServices(string service1, string service2)
        {
            WhenTheUserSelectsASpecificStreamingService(service1);
            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss)).Any());
            WhenTheUserSelectsASpecificStreamingService(service2);
        }

        [When("the user clicks the \"Clear Filters\" button")]
        public void WhenTheUserClicksTheClearFiltersButton()
        {
            var btn = _driver.FindElement(By.Id("clearFilters"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].click(); arguments[0].dispatchEvent(new Event('change', { bubbles: true }));",
                btn);
        }

        [Then("the content list is updated to show only items available on \"(.*)\"")]
        public void ThenTheContentListIsUpdatedToShowOnlyItemsAvailableOn(string service)
        {
            var cards = _driver.FindElements(By.CssSelector(MovieCardCss)).Where(c => c.Displayed);
            Assert.IsTrue(cards.Any(), "No content after filter");
            foreach (var card in cards)
            {
                var list = card.GetAttribute("data-streaming").Split(',').Select(s => s.Trim());
                Assert.Contains(service, list.ToList(), $"Card missing '{service}'");
            }
        }

        [Then("the content list is updated to show items available on either \"(.*)\" or \"(.*)\"")]
        public void ThenTheContentListIsUpdatedToShowItemsAvailableOnEither(string service1, string service2)
        {
            var cards = _driver.FindElements(By.CssSelector(MovieCardCss)).Where(c => c.Displayed);
            Assert.IsTrue(cards.Any(), "No content after multi-filter");
            foreach (var card in cards)
            {
                var list = card.GetAttribute("data-streaming").Split(',').Select(s => s.Trim());
                Assert.IsTrue(list.Contains(service1) || list.Contains(service2),
                    $"Card missing both '{service1}' and '{service2}'");
            }
        }

        [Then("all streaming service filters are removed")]
        public void ThenAllStreamingServiceFiltersAreRemoved()
        {
            var checkboxes = _driver.FindElements(By.CssSelector(StreamingFiltersCss));
            foreach (var cb in checkboxes)
                Assert.IsFalse(cb.Selected, "Filter not cleared");
        }

        [Then("the content list returns to the default unfiltered view")]
        public void ThenTheContentListReturnsToTheDefaultUnfilteredView()
        {
            Assert.IsTrue(_driver.FindElements(By.CssSelector(MovieCardCss)).Count > 0,
                "Default view empty");
        }

        [Then("the content list should update within (.*) seconds")]
        public void ThenTheContentListShouldUpdateWithinSeconds(int seconds)
        {
            var start = DateTime.Now;
            _wait.Timeout = TimeSpan.FromSeconds(seconds);
            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss)).Any());
            Assert.LessOrEqual((DateTime.Now - start).TotalSeconds, seconds,
                $"Update took longer than {seconds} seconds");
        }
    }
}
