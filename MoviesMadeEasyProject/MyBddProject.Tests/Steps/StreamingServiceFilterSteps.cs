using NUnit.Framework;
using Reqnroll;
using Reqnroll.BoDi;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;

[Binding]
public class StreamingServiceFilterSteps
{
    private readonly IWebDriver _driver;

    public StreamingServiceFilterSteps(IWebDriver driver)
    {
        _driver = driver;
    }

    [Given("the user is on the content browsing page")]
    public void GivenTheUserIsOnTheContentBrowsingPage()
    {
        _driver.Navigate().GoToUrl("http://localhost:5000");
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElement(By.Id("searchInput")).Displayed);
        Assert.IsTrue(_driver.FindElement(By.Id("searchInput")).Displayed);
    }

    [Given("the list of available streaming services is displayed")]
    public void GivenTheListOfAvailableStreamingServicesIsDisplayed()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var dropdownButton = wait.Until(d => d.FindElement(By.Id("sortGenreDropdown")));
        dropdownButton.Click();

        var dropdownMenu = wait.Until(d => d.FindElement(By.CssSelector("ul.dropdown-menu[aria-labelledby='sortGenreDropdown']")));
        Assert.IsTrue(dropdownMenu.Displayed, "The streaming services dropdown menu is not displayed.");

        var listItems = dropdownMenu.FindElements(By.TagName("li"));
        Assert.IsTrue(listItems.Count > 0, "No streaming services were found in the dropdown.");
    }

    [Given("the user has one or more streaming service filters applied")]
    public void GivenTheUserHasOneOrMoreStreamingServiceFiltersApplied()
    {
        GivenTheUserIsOnTheContentBrowsingPage();

        var searchInput = _driver.FindElement(By.Id("searchInput"));
        searchInput.Clear();
        searchInput.SendKeys("Titanic");
        _driver.FindElement(By.CssSelector("button.search-btn")).Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        wait.Until(d => {
            try
            {
                var results = d.FindElement(By.Id("results"));
                return results.Displayed && !string.IsNullOrWhiteSpace(results.Text);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });

        wait.Until(d => {
            try
            {
                var filtersContainer = d.FindElement(By.Id("streaming-filters"));
                return filtersContainer.Displayed &&
                      filtersContainer.FindElements(By.CssSelector("input[type='checkbox']")).Count > 0;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });

        System.Threading.Thread.Sleep(1000);

        try
        {
            var serviceCheckbox = wait.Until(driver => {
                try
                {
                    var elem = driver.FindElement(By.CssSelector("input[value='Pluto TV']"));
                    if (elem.Displayed && elem.Enabled)
                    {
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", elem);
                        return elem;
                    }
                    return null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });

            if (serviceCheckbox != null)
            {
                System.Threading.Thread.Sleep(500);

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", serviceCheckbox);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", serviceCheckbox);
            }
            else
            {
                throw new Exception("Could not find or interact with the streaming service checkbox");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting streaming service: {ex.Message}");
            throw;
        }
    }

    [When("the user selects a specific streaming service \"(.*)\"")]
    public void WhenTheUserSelectsASpecificStreamingService(string service)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var serviceCheckbox = wait.Until(driver => {
            try
            {
                var elem = driver.FindElement(By.CssSelector($"input[value='{service}']"));
                return (elem.Displayed && elem.Enabled) ? elem : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });

        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", serviceCheckbox);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", serviceCheckbox);
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", serviceCheckbox);
        ((IJavaScriptExecutor)_driver).ExecuteScript("updateClearFiltersVisibility();");
    }

    [When("the user selects streaming services \"(.*)\" and \"(.*)\"")]
    public void WhenTheUserSelectsStreamingServices(string service1, string service2)
    {
        var service1Checkbox = _driver.FindElement(By.CssSelector($"input[value='{service1}']"));
        var service2Checkbox = _driver.FindElement(By.CssSelector($"input[value='{service2}']"));
        service1Checkbox.Click();
        service2Checkbox.Click();
    }

    [When("the user clicks the \"Clear Filters\" button")]
    public void WhenTheUserClicksTheClearFiltersButton()
    {
        var clearFiltersButton = _driver.FindElement(By.Id("clearFilters"));
        clearFiltersButton.Click();
    }

    [Then("the content list is updated to show only items available on \"(.*)\"")]
    public void ThenTheContentListIsUpdatedToShowOnlyItemsAvailableOn(string service)
    {
        var contentItems = _driver.FindElements(By.CssSelector(".content-item"));
        foreach (var item in contentItems)
        {
            Assert.IsTrue(item.Text.Contains(service));
        }
    }

    [Then("the content list is updated to show items available on either \"(.*)\" or \"(.*)\"")]
    public void ThenTheContentListIsUpdatedToShowItemsAvailableOnEither(string service1, string service2)
    {
        var contentItems = _driver.FindElements(By.CssSelector(".content-item"));
        foreach (var item in contentItems)
        {
            Assert.IsTrue(item.Text.Contains(service1) || item.Text.Contains(service2));
        }
    }

    [Then("all streaming service filters are removed")]
    public void ThenAllStreamingServiceFiltersAreRemoved()
    {
        var checkboxes = _driver.FindElements(By.CssSelector("input[type='checkbox']"));
        foreach (var checkbox in checkboxes)
        {
            Assert.IsFalse(checkbox.Selected);
        }
    }

    [Then("the content list returns to the default unfiltered view")]
    public void ThenTheContentListReturnsToTheDefaultUnfilteredView()
    {
        var contentItems = _driver.FindElements(By.CssSelector(".content-item"));
        Assert.IsTrue(contentItems.Count > 0);
    }

    [Then("the content list should update within (.*) seconds")]
    public void ThenTheContentListShouldUpdateWithinSeconds(int seconds)
    {
        var startTime = DateTime.Now;
        var contentItems = _driver.FindElements(By.CssSelector(".content-item"));
        var endTime = DateTime.Now;
        Assert.IsTrue((endTime - startTime).TotalSeconds <= seconds);
        Assert.IsTrue(contentItems.Count > 0);
    }

    [When("the user selects a streaming service filter")]
    public void WhenTheUserSelectsAStreamingServiceFilter()
    {
        WhenTheUserSelectsASpecificStreamingService("Pluto TV");
    }
}