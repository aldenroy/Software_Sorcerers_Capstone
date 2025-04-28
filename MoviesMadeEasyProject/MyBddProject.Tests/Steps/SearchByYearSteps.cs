using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.PageObjects;
using System;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class SearchByYearSteps
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const string CardCss = ".movie-card";
        private const string YearCss = ".movie-year";

        public SearchByYearSteps(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Given(@"the items listing page displays items from various years")]
        public void GivenTheItemsListingPageDisplaysItemsFromVariousYears()
        {
            var input = _driver.FindElement(By.Id("searchInput"));
            input.Clear();
            input.SendKeys("Titanic");
            input.SendKeys(Keys.Enter);

            _wait.Until(driver => driver.FindElements(By.CssSelector(CardCss)).Any());

            var years = _driver.FindElements(By.CssSelector(YearCss))
                .Select(e => Regex.Match(e.Text, @"\d{4}").Value)
                .Distinct()
                .ToList();

            Assert.That(years.Count, Is.GreaterThan(1),
                "Expected items from more than one year");
        }

        [When(@"I set the min year slider to ""(.*)"" and the max year slider to ""(.*)""")]
        public void WhenISetMinAndMaxYearSliders(string minYear, string maxYear)
        {
            var minSlider = _driver.FindElement(By.Id("minYear"));
            var maxSlider = _driver.FindElement(By.Id("maxYear"));

            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                arguments[0].value = arguments[2];
                arguments[1].value = arguments[3];
                arguments[0].dispatchEvent(new Event('input'));
                arguments[0].dispatchEvent(new Event('change'));
                arguments[1].dispatchEvent(new Event('input'));
                arguments[1].dispatchEvent(new Event('change'));
            ", minSlider, maxSlider, minYear, maxYear);

            _wait.Until(driver => driver.FindElements(By.CssSelector(CardCss))
                .All(card =>
                {
                    var text = card.FindElement(By.CssSelector(YearCss)).Text;
                    var y = int.Parse(Regex.Match(text, @"\d{4}").Value);
                    return y >= int.Parse(minYear) && y <= int.Parse(maxYear);
                }));
        }

[Then(@"only items between year ""(.*)"" and ""(.*)"" are displayed")]
public void ThenOnlyItemsBetweenYearsAreDisplayed(string minYear, string maxYear)
{
    var min = int.Parse(minYear);
    var max = int.Parse(maxYear);
    var cards = _driver.FindElements(By.CssSelector(CardCss));
    Assert.That(cards, Is.Not.Empty, "No items displayed after filtering");

    foreach (var card in cards)
    {
        var text = card.FindElement(By.CssSelector(YearCss)).Text;
        var y = int.Parse(Regex.Match(text, @"\d{4}").Value);
        Assert.That(y, Is.InRange(min, max),
            $"Found an item outside the range {min}-{max}: {y}");
    }
}
    }
}