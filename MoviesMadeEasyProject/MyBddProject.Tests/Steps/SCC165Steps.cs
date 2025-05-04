using NUnit.Framework;
using Reqnroll;
using Reqnroll.BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class OffCanvasFilterSteps
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const string FilterToggleCss = "button[data-bs-toggle='offcanvas']";
        private const string FilterPanelCss = "#filtersOffcanvas";
        private const string OverlayCss = ".offcanvas-backdrop";
        private const string MovieCardCss = ".movie-card";
        private const string ClearFiltersCss = "#clearFilters";
        private const string ApplyFiltersCss = "button[onclick='applyFilters()']";
        private const string MinYearCss = "#minYear";
        private const string MaxYearCss = "#maxYear";
        private int _initialMovieCount;

        public OffCanvasFilterSteps(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Given("the user is on the content browsing page using a desktop browser")]
        public void GivenTheUserIsOnContentBrowsingPageDesktop()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000");
            Assert.IsTrue(_driver.FindElement(By.Id("searchInput")).Displayed,
                "Search input should be visible on desktop");
        }

        [Then("the filter toggle button is visible in the header")]
        public void ThenFilterToggleVisible()
        {
            var toggle = _driver.FindElement(By.CssSelector(FilterToggleCss));
            Assert.IsTrue(toggle.Displayed, "Filter toggle should be visible");
        }

        [Then("the filter panel is hidden off-canvas")]
        public void ThenFilterPanelHiddenOffCanvas()
        {
            var panel = _driver.FindElement(By.CssSelector(FilterPanelCss));
            var classes = panel.GetAttribute("class");
            Assert.IsTrue(classes.Contains("offcanvas-start"), "Filter panel should be off-canvas by default");
            Assert.IsFalse(classes.Contains("show"), "Filter panel should not be shown by default");
        }

        [When("the user clicks the filter toggle button")]
        public void WhenUserClicksFilterToggle()
        {
            _driver.FindElement(By.CssSelector(FilterToggleCss)).Click();
        }

        [Then("the filter panel slides in from the left as an off-canvas panel")]
        public void ThenFilterPanelSlidesIn()
        {
            _wait.Until(d => {
                var panel = d.FindElement(By.CssSelector(FilterPanelCss));
                var classes = panel.GetAttribute("class");
                return classes.Contains("show") && panel.Displayed;
            });
            Assert.IsTrue(_driver.FindElement(By.CssSelector(FilterPanelCss)).Displayed,
                "Filter panel should be visible after toggle");
        }

        [Then("the main content is dimmed behind the panel")]
        public void ThenMainContentDimmed()
        {
            _wait.Until(d => d.FindElements(By.CssSelector(OverlayCss)).Any());
            var overlay = _driver.FindElement(By.CssSelector(OverlayCss));
            Assert.IsTrue(overlay.Displayed, "Overlay should be displayed when panel is open");
        }

        [Given("the filter panel is hidden off-canvas")]
        public void GivenTheFilterPanelIsHiddenOffCanvas()
        {
            // offcanvas is Bootstrap: it has class "show" when visible
            var panel = _driver.FindElement(By.Id("filtersOffcanvas"));
            Assert.IsFalse(panel.GetAttribute("class").Contains("show"),
                          "Filter panel should start hidden off-canvas");
        }

        [When("the user selects one or more filter options")]
        public void WhenTheUserSelectsOneOrMoreFilterOptions()
        {
            // 1) Kick off a search so we have cards to filter
            var input = _driver.FindElement(By.Id("searchInput"));
            input.Clear();
            input.SendKeys("a");
            input.SendKeys(Keys.Enter);

            // 2) Wait for results to appear
            _wait.Until(d =>
                d.FindElements(By.CssSelector(".movie-card"))
                 .Any(c => c.Displayed));

            // 3) Ensure the offcanvas panel is open
            var panel = _driver.FindElement(By.Id("filtersOffcanvas"));
            _wait.Until(d => panel.GetAttribute("class").Contains("show"));
            // optional extra assert to give a clear error message:
            Assert.IsTrue(panel.GetAttribute("class").Contains("show"), "Filter panel did not open");


            // 4) Tick the first available filter checkbox
            var checkbox = _driver.FindElements(By.CssSelector("#genre-filters input[type='checkbox']"))
                                  .FirstOrDefault();
            Assert.NotNull(checkbox, "No filter checkboxes found");
            checkbox.Click();

            // 5) Wait for at least one movie card to disappear
            _wait.Until(d =>
            {
                var all = d.FindElements(By.CssSelector(".movie-card")).ToList();
                var visible = all.Where(c => c.Displayed).ToList();
                return visible.Count < all.Count;
            });
        }

        [Then("the content list updates accordingly based on the selected filters")]
        public void ThenTheContentListUpdatesAccordinglyBasedOnTheSelectedFilters()
        {
            var allCards = _driver.FindElements(By.CssSelector(".movie-card")).ToList();
            var visibleCards = allCards.Where(c => c.Displayed).ToList();

            Assert.IsTrue(visibleCards.Any(), "No movies are visible after filtering");
            Assert.Less(visibleCards.Count, allCards.Count,
                        "Filtering did not reduce the number of visible movies");
        }

        [Given("the user has one or more filters applied")]
        public void GivenFiltersApplied()
        {
            // 1) Perform initial search
            var input = _driver.FindElement(By.Id("searchInput"));
            input.Clear();
            input.SendKeys("a");
            input.SendKeys(Keys.Enter);
            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss))
                             .Any(c => c.Displayed));

            // 2) RECORD the unfiltered total count here
            _initialMovieCount = _driver
                .FindElements(By.CssSelector(MovieCardCss))
                .Count;

            // 3) Open the off-canvas panel
            _driver.FindElement(By.CssSelector(FilterToggleCss)).Click();
            _wait.Until(d => d.FindElement(By.CssSelector(FilterPanelCss))
                             .GetAttribute("class")
                             .Contains("show"));

            // 4) Click the first streaming-service checkbox
            var firstCb = _driver
                .FindElement(By.CssSelector("#streaming-filters input[type='checkbox']"));
            firstCb.Click();
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].dispatchEvent(new Event('change', { bubbles: true }));", firstCb);

            // 5) Wait until at least one card is hidden
            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss))
                             .Any(c => !c.Displayed));
        }

        [When(@"the user clicks the ""Clear Filters"" button in off-canvas panel")]
        public void WhenUserClicksClearFilters()
        {
            // 1) JS?click the Clear Filters button
            var clearBtn = _driver.FindElement(By.CssSelector(ClearFiltersCss));
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].click();", clearBtn);

            // 2) Wait for the loading spinner to appear & then disappear
            _wait.Until(d =>
                d.FindElement(By.Id("loadingSpinner"))
                 .GetCssValue("display") != "none");
            _wait.Until(d =>
                d.FindElement(By.Id("loadingSpinner"))
                 .GetCssValue("display") == "none");

            // 3) Now *also* wait for every card's inline style to clear out "display: none"
            _wait.Until(d =>
                d.FindElements(By.CssSelector(MovieCardCss))
                 .All(card =>
                 {
                     var style = card.GetAttribute("style") ?? "";
                     return !style.Contains("display: none");
                 })
            );
        }

        [Then("all applied filters are removed and the content list returns to the default view")]
        public void ThenClearFiltersResetsView()
        {
            // 1) Clear Filters button should be hidden
            var clearBtn = _driver.FindElement(By.CssSelector(ClearFiltersCss));
            Assert.IsFalse(clearBtn.Displayed, "Clear Filters button should be hidden after clearing.");

            // 2) All genre checkboxes unchecked
            var genreCbs = _driver.FindElements(By.CssSelector("#genre-filters input[type='checkbox']"));
            Assert.IsTrue(genreCbs.All(cb => !cb.Selected), "Genre filters should all be unchecked.");

            // 3) All streaming checkboxes unchecked
            var streamCbs = _driver.FindElements(By.CssSelector("#streaming-filters input[type='checkbox']"));
            Assert.IsTrue(streamCbs.All(cb => !cb.Selected), "Streaming filters should all be unchecked.");

            // 4) Year sliders reset to default
            var minSlider = _driver.FindElement(By.CssSelector(MinYearCss));
            var maxSlider = _driver.FindElement(By.CssSelector(MaxYearCss));
            Assert.AreEqual(minSlider.GetAttribute("value"), minSlider.GetAttribute("min"),
                "Min Year slider should reset to its minimum.");
            Assert.AreEqual(maxSlider.GetAttribute("value"), maxSlider.GetAttribute("max"),
                "Max Year slider should reset to its maximum.");

            // 5) Finally, ensure the content area has repopulated at least one movie card
            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss)).Any(c => c.Displayed));
            var visibleCount = _driver.FindElements(By.CssSelector(MovieCardCss))
                                      .Count(c => c.Displayed);
            Assert.IsTrue(visibleCount > 0, "Expected at least one movie visible after clearing filters.");
        }

        [When("the user sets the \"Min Year\" slider to (\\d+)")]
        public void WhenUserSetsMinYear(int year)
        {
            var slider = _driver.FindElement(By.CssSelector(MinYearCss));
            ((IJavaScriptExecutor)_driver).ExecuteScript($"arguments[0].value = {year}; arguments[0].dispatchEvent(new Event('input'));", slider);
        }
        
        [When("the user sets the \"Max Year\" slider to (\\d+)")]
        public void WhenUserSetsMaxYear(int year)
        {
            var slider = _driver.FindElement(By.CssSelector(MaxYearCss));
            ((IJavaScriptExecutor)_driver).ExecuteScript($"arguments[0].value = {year}; arguments[0].dispatchEvent(new Event('input'));", slider);
        }

        [When("the user clicks the \"Apply Filters\" button")]
        public void WhenUserClicksApply()
        {
            _driver.FindElement(By.CssSelector(ApplyFiltersCss)).Click();
            _wait.Until(d => d.FindElements(By.CssSelector(MovieCardCss)).Any());
        }
        
        [Then("only movies released between (\\d+) and (\\d+) are displayed in the content list")]
        public void ThenMoviesWithinYearRange(int min, int max)
        {
            var cards = _driver.FindElements(By.CssSelector(MovieCardCss)).Where(c => c.Displayed);
            foreach (var card in cards)
            {
                var text = card.FindElement(By.CssSelector(".movie-year")).Text;
                int y = int.Parse(text.Trim('(', ')'));
                Assert.IsTrue(y >= min && y <= max,
                    $"Movie year {y} not within range {min}-{max}");
            }
        }

        [Given("the user accesses the content browsing page on a supported browser")]
        public void GivenUserAccessesOnSupportedBrowser()
        {
            GivenTheUserIsOnContentBrowsingPageDesktop();
        }

        [Then("the filter panel is consistently positioned on the left-hand side and functions as expected")]
        public void ThenConsistentLayoutAndFunction()
        {
            ThenFilterToggleVisible();
            ThenFilterPanelHiddenOffCanvas();
        }

        [When(@"the user clicks the ""Apply"" button")]
        public void WhenTheUserClicksApplyButton()
        {
            // Find the Apply Filters button (calls your applyFilters() JS)
            var applyBtn = _driver.FindElement(
                By.CssSelector("button[onclick='applyFilters()']"));
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("arguments[0].click();", applyBtn);

            // Wait for the offcanvas to close (Bootstrap removes "show")
            _wait.Until(d =>
                !_driver
                  .FindElement(By.Id("filtersOffcanvas"))
                  .GetAttribute("class")
                  .Contains("show"));
        }
    }
}
