using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace MyBddProject.Tests.PageObjects
{
    public class DashboardPriceSetup
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public DashboardPriceSetup(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        public void NavigateToSubscriptionForm()
        {
            var manageButton = _wait.Until(d => d.FindElement(By.Id("add-subscription-btn")));
            manageButton.Click();

            _wait.Until(d => d.Url.Contains("SubscriptionForm"));
        }

        public void ClickToggleAnalysisButton()
        {
            var toggleButton = _wait.Until(d => d.FindElement(By.Id("toggle-analysis-btn")));
            toggleButton.Click();
        }

        public bool IsPriceDisplayed(string serviceName)
        {
            var serviceItem = FindServiceItem(serviceName);
            var priceElement = serviceItem.FindElement(By.CssSelector(".subscription-price"));
            return !priceElement.GetAttribute("class").Contains("d-none");
        }

        public string GetDisplayedPrice(string serviceName)
        {
            var serviceItem = FindServiceItem(serviceName);
            var priceElement = serviceItem.FindElement(By.CssSelector(".subscription-price"));
            return priceElement.Text.Trim();
        }

        public void ClearPriceInput(string serviceName)
        {
            var priceInput = GetPriceInput(serviceName);
            priceInput.Clear();
        }

        public void EnterPrice(string serviceName, string price)
        {
            var priceInput = GetPriceInput(serviceName);
            priceInput.Clear();
            priceInput.SendKeys(price);
        }

        public void FocusPriceInput(string serviceName)
        {
            var priceInput = GetPriceInput(serviceName);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].focus();", priceInput);
        }

        public bool IsPriceInputFocused(string serviceName)
        {
            var priceInput = GetPriceInput(serviceName);
            var activeElement = _driver.SwitchTo().ActiveElement();
            return priceInput.Equals(activeElement);
        }

        public string GetTotalMonthlySpend()
        {
            var totalElement = _wait.Until(d => d.FindElement(By.Id("subscription-total")));
            return totalElement.Text.Replace("Subscription's Total Cost:", "").Trim();
        }

        public bool IsTotalDisplayed()
        {
            var totalElement = _driver.FindElement(By.Id("subscription-total"));
            return !totalElement.GetAttribute("class").Contains("d-none");
        }

        public string GetPriceInputAriaLabel(string serviceName)
        {
            var priceInput = GetPriceInput(serviceName);
            return priceInput.GetAttribute("aria-label");
        }

        private IWebElement FindServiceItem(string serviceName)
        {
            return _wait.Until(d =>
            {
                var items = d.FindElements(By.CssSelector(".subscription-item"));
                foreach (var item in items)
                {
                    try
                    {
                        var imgAlt = item.FindElement(By.TagName("img")).GetAttribute("alt");
                        if (imgAlt.Contains(serviceName, StringComparison.OrdinalIgnoreCase))
                        {
                            return item;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                    }
                }
                return null;
            });
        }

        private IWebElement GetPriceInput(string serviceName)
        {
            if (!_driver.Url.Contains("SubscriptionForm"))
            {
                NavigateToSubscriptionForm();
            }

            var serviceCard = _wait.Until(d =>
            {
                var cards = d.FindElements(By.CssSelector(".card"));
                foreach (var card in cards)
                {
                    try
                    {
                        var cardText = card.FindElement(By.CssSelector(".card-text")).Text;
                        if (cardText.Contains(serviceName, StringComparison.OrdinalIgnoreCase))
                        {
                            return card;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                    }
                }
                return null;
            });

            return serviceCard.FindElement(By.CssSelector(".price-input"));
        }

        public void SubmitForm()
        {
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    var form = document.getElementById('subscriptionForm');
                    if(form) {
                        form.submit();
                    }
                ");

                _wait.Until(d => d.Url.Contains("/Dashboard"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitForm: {ex.Message}");

                var submitButtons = _driver.FindElements(By.CssSelector("button[type='submit']"));
                if (submitButtons.Count > 0)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submitButtons[0]);
                    _wait.Until(d => d.Url.Contains("/Dashboard"));
                }
            }
        }
    }
}