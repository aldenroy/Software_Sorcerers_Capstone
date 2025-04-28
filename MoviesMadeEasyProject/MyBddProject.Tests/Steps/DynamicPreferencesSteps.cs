using System;
using OpenQA.Selenium;
using Reqnroll;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.Tests.PageObjects;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class DynamicPreferencesSteps
    {
        private readonly IWebDriver _driver;
        private PreferencesPageTestSetup _preferencesPage;

        public DynamicPreferencesSteps(IWebDriver driver)
        {
            _driver = driver;
            _preferencesPage = new PreferencesPageTestSetup(_driver);
        }

        [Given(@"the user is logged in and on the Preferences page")]
        public void GivenTheUserIsOnThePreferencesPage()
        {
            try {
                // First, navigate to the login page
                _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");
                
                // Wait for the page to load
                new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                    .Until(d => d.Title.Contains("Log in") || d.Url.Contains("Login"));
                
                // Enter login credentials (use test account credentials)
                // Use Input_Email instead of Email based on the ASP.NET form structure
                _driver.FindElement(By.Id("Input_Email")).SendKeys("test@test.com");
                _driver.FindElement(By.Id("Input_Password")).SendKeys("Test!123");
                
                // Click the login button - using its specific ID from the HTML
                _driver.FindElement(By.Id("login-submit")).Click();
                
                // Wait for successful login (might need to adjust based on your app's behavior)
                new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                    .Until(d => d.Url.Contains("Dashboard") || d.Title.Contains("Dashboard"));
                
                // Now navigate to the preferences page - check the actual URL for preferences
                _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Preferences"); // You might need to adjust this URL
                
                // Wait for the preferences page to load
                new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                    .Until(d => d.Title.Contains("Preferences") || d.Url.Contains("Preferences"));
                
            } catch (Exception ex) {
                Console.WriteLine("Error during login or navigation: " + ex.Message);
                Console.WriteLine("Current URL: " + _driver.Url);
                throw;
            }
        }

        [When(@"the user selects ""(.*)"" from the font size dropdown")]
        public void WhenTheUserSelectsFromTheFontSizeDropdown(string fontSize)
        {
            _preferencesPage.SelectFontSize(fontSize);
        }

        [Then(@"the font size should immediately switch to large")]
        public void ThenTheFontSizeShouldImmediatelySwitchToLarge()
        {
            var body = _driver.FindElement(By.TagName("body"));
            Assert.That(body.GetCssValue("font-size"), Is.EqualTo("large") 
                .Or.EqualTo("24px") // Adjust based on your implementation
                .Or.Contains("large"));
        }

        [When(@"the user selects ""(.*)"" from the font type dropdown")]
        public void WhenTheUserSelectsFromTheFontTypeDropdown(string fontType)
        {
            _preferencesPage.SelectFontType(fontType);
        }

        [Then(@"the font type should immediately switch to open dyslexic")]
        public void ThenTheFontTypeShouldImmediatelySwitchToOpenDyslexic()
        {
            var body = _driver.FindElement(By.TagName("body"));
            Assert.That(body.GetCssValue("font-family"), Contains.Substring("Open-Dyslexic, sans-serif")
                .Or.Contains("open dyslexic"));
        }

        [When(@"the user selects ""(.*)"" from the Theme dropdown")]
        public void WhenTheUserSelectsFromTheThemeDropdown(string theme)
        {
            _preferencesPage.SelectColorMode(theme);
        }

        [Then(@"the page should immediately switch to dark mode")]
        public void ThenThePageShouldImmediatelySwitchToDarkMode()
        {
            var body = _driver.FindElement(By.TagName("body"));
            string backgroundColor = body.GetCssValue("background-color");
            Console.WriteLine("Actual background-color: " + backgroundColor);

            bool isDarkTheme = _driver.FindElement(By.TagName("html")).GetAttribute("data-theme") == "dark" || 
                            _driver.FindElement(By.TagName("body")).GetAttribute("class").Contains("dark-theme");
            
            Assert.That(isDarkTheme, Is.True);
        }

        [Then(@"the ""(.*)"" logo should appear")]
        public void ThenTheLogoShouldAppear(string logoFile)
        {
            // Check for the dark mode logo
            var logo = _driver.FindElement(By.CssSelector("img[src*='" + logoFile + "']"));
            Assert.That(logo.Displayed, Is.True, "Dark mode logo is not displayed");
        }
    }
}
