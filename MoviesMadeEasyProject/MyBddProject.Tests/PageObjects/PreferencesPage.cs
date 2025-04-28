using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.Tests.PageObjects;
using Reqnroll;

namespace MyBddProject.Tests.PageObjects
{
    public class PreferencesPageTestSetup
    {
        private readonly IWebDriver _driver;

        public PreferencesPageTestSetup(IWebDriver driver)
        {
            _driver = driver;
        }

        private IWebElement WaitForElement(By by)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            return wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return (element.Displayed && element.Enabled) ? element : null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
        }

        public void SelectColorMode(string colorMode)
        {
            var colorModeDropdown = new SelectElement(WaitForElement(By.Id("ColorMode")));
            colorModeDropdown.SelectByText(colorMode);
        }

        public void SelectFontSize(string fontSize)
        {
            var fontSizeDropdown = new SelectElement(WaitForElement(By.Id("FontSize")));
            fontSizeDropdown.SelectByText(fontSize);
        }

        public void SelectFontType(string fontType)
        {
            var fontTypeDropdown = new SelectElement(WaitForElement(By.Id("FontType")));
            fontTypeDropdown.SelectByText(fontType);
        }

        public void SavePreferences()
        {
            var saveButton = WaitForElement(By.XPath("//button[@type='submit' and @aria-label='Save Preferences']"));
            saveButton.Click();
        }

        public void SkipPreferences()
        {
            var skipButton = WaitForElement(By.XPath("//button[@type='submit' and @formaction and contains(text(), 'Skip')]"));
            skipButton.Click();
        }
    }
}
