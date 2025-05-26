using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace YourProject.Tests.Steps
{
    [Binding]
    public class RatingFilterSteps
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private List<string> _initialItems;

        public RatingFilterSteps(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Given(@"I am on the ""([^""]*)"" page")]
        public void GivenIAmOnThePage(string pageName)
        {
            _driver.Navigate().GoToUrl("http://localhost:5000"); // adjust base URL if needed
            _wait.Until(d => d.Title.Contains(pageName));
        }

        [Given(@"a list of items with ratings out of 100 is displayed")]
        public void GivenAListOfItemsWithRatingsOutOf100IsDisplayed()
        {
            var searchInput = _driver.FindElement(By.Id("searchInput"));
            searchInput.SendKeys("a");
            var searchButton = _driver.FindElement(By.CssSelector("button[aria-label='Search movies']"));
            searchButton.Click();
            _wait.Until(d => d.FindElements(By.CssSelector(".movie-card")).Count > 0);

            _initialItems = _driver.FindElements(By.CssSelector(".movie-card"))
                .Select(card => card.Text)
                .ToList();
        }

        [Given(@"the rating-range inputs is visible")]
        public void GivenTheRatingRangeInputsIsVisible()
        {
            // Open the filters offcanvas
            var toggle = _driver.FindElement(By.CssSelector("button[data-bs-target='#filtersOffcanvas']"));
            toggle.Click();

            // Wait until the panel is fully shown
            var panel = _wait.Until(d =>
                d.FindElement(By.Id("filtersOffcanvas")).GetAttribute("class").Contains("show")
            );

            // Wait until both sliders are visible
            var minSlider = _wait.Until(d =>
            {
                var el = d.FindElement(By.Id("minRating"));
                return el.Displayed ? el : null;
            });

            var maxSlider = _wait.Until(d =>
            {
                var el = d.FindElement(By.Id("maxRating"));
                return el.Displayed ? el : null;
            });

            Assert.IsTrue(minSlider.Displayed, "Min rating slider should be visible");
            Assert.IsTrue(maxSlider.Displayed, "Max rating slider should be visible");
        }


        [Given(@"the minimum rating is set to (\d+)")]
        public void GivenTheMinimumRatingIsSetTo(int minValue)
        {
            var slider = _driver.FindElement(By.Id("minRating"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('change'));",
                slider, minValue
            );
        }

        [Given(@"the maximum rating is set to (\d+)")]
        public void GivenTheMaximumRatingIsSetTo(int maxValue)
        {
            var slider = _driver.FindElement(By.Id("maxRating"));
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].value = arguments[1]; arguments[0].dispatchEvent(new Event('change'));",
                slider, maxValue
            );
        }
        [Then(@"I should see only items with ratings ≥ (\d+) and ≤ (\d+)")]
        public void ThenIShouldSeeOnlyItemsWithRatingsAnd(int minValue, int maxValue)
        {
            var cards = _driver.FindElements(By.CssSelector(".movie-card"));
            int checkedCount = 0;

            foreach (var card in cards)
            {
                string ratingText = "";
                try
                {
                    ratingText = card.FindElement(By.CssSelector(".movie-rating")).Text.Trim(); // e.g., "Rating: 84.5"
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Warning: card missing .movie-rating element.");
                    continue; // skip this card
                }

                if (string.IsNullOrWhiteSpace(ratingText))
                {
                    Console.WriteLine("Warning: card has empty rating text.");
                    continue;
                }

                var match = Regex.Match(ratingText, @"\d+(\.\d+)?");
                if (!match.Success)
                {
                    Console.WriteLine($"Warning: Could not parse rating from text: '{ratingText}'");
                    continue;
                }

                var rating = float.Parse(match.Value);
                Assert.That(rating,
                    Is.GreaterThanOrEqualTo(minValue).And.LessThanOrEqualTo(maxValue),
                    $"Rating {rating} was not between {minValue} and {maxValue}");

                checkedCount++;
            }

            Assert.Greater(checkedCount, 0, "No movie ratings were checked.");
        }
        [Then(@"I should see an inline validation error ""([^""]*)""")]
        public void ThenIShouldSeeAnInlineValidationError(string expected)
        {
            var error = _driver.FindElement(By.CssSelector(".error-message[role='alert']"));
            Assert.That(error.Text.Trim(), Is.EqualTo(expected));
        }
        [Given(@"no items have ratings between (\d+) and (\d+)")]
        public void GivenNoItemsHaveRatingsBetweenAnd(int minValue, int maxValue)
        {
            var cards = _driver.FindElements(By.CssSelector(".movie-card"));
            foreach (var card in cards)
            {
                var rating = int.Parse(Regex.Match(
                    card.FindElement(By.CssSelector(".movie-rating")).Text,
                    "\\d+").Value);
                Assert.That(rating < minValue || rating > maxValue,
                    $"Found an item with rating {rating} between {minValue} and {maxValue}");
            }
        }

[Then(@"I should see ""(.*)"" in the results container")]
public void ThenIShouldSeeInTheResultsContainer(string expectedText)
{
    // 1) Wait for the #results element to exist & be visible
    var resultsEl = _wait.Until(d =>
    {
        var el = d.FindElement(By.Id("results"));
        return el.Displayed ? el : null;
    });

    // 2) Wait until its .Text equals exactly the expected message
    _wait.Until(d => resultsEl.Text.Trim().Equals(expectedText));

    // 3) Final assert (just in case)
    Assert.AreEqual(expectedText, resultsEl.Text.Trim());
}
    }
}