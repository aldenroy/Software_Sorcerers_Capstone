using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MyBddProject.Tests.PageObjects
{
    public class ContentBrowsingPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public ContentBrowsingPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        public bool AreStreamingFiltersDisplayed()
        {
            try
            {
                var streamingFilters = _driver.FindElement(By.Id("streaming-filters"));
                return streamingFilters.Displayed;
            }
            catch
            {
                return false;
            }
        }

        public void SelectStreamingService(string service)
        {
            var button = _driver.FindElement(By.CssSelector($"button[data-testid='filter-{service}']"));
            button.Click();
        }

        public void ClickClearFiltersButton()
        {
            var button = _driver.FindElement(By.XPath("//button[text()='Clear Filters']"));
            button.Click();
        }

        public bool AreActiveFiltersPresent()
        {
            return _driver.FindElements(By.CssSelector("button.active[data-testid^='filter-']")).Count > 0;
        }

        public bool AreAllFiltersRemoved()
        {
            return _driver.FindElements(By.CssSelector("button.active[data-testid^='filter-']")).Count == 0;
        }

        public int CountContentItems()
        {
            return _driver.FindElements(By.CssSelector("[data-testid='content-item']")).Count;
        }

        public bool DoAllItemsContainService(string service)
        {
            var contentItems = _driver.FindElements(By.CssSelector("[data-testid='content-item']"));
            return contentItems.All(item => item.Text.Contains(service));
        }

        public bool DoAllItemsContainEitherService(string service1, string service2)
        {
            var contentItems = _driver.FindElements(By.CssSelector("[data-testid='content-item']"));
            return contentItems.All(item => 
                item.Text.Contains(service1) || item.Text.Contains(service2));
        }

        public TimeSpan MeasureTimeToApplyFilter(string service)
        {
            var stopwatch = Stopwatch.StartNew();
            SelectStreamingService(service);
            
            _wait.Until(d => d.FindElements(By.CssSelector("[data-testid='content-item']")).Count > 0);
            stopwatch.Stop();
            
            return stopwatch.Elapsed;
        }
    }
}