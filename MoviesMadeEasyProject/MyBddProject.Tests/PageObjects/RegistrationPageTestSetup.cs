using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.Tests.PageObjects
{
    public class RegistrationPageTestSetup
    {
        private readonly IWebDriver _driver;

        public RegistrationPageTestSetup(IWebDriver driver)
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

        public void FillFirstName(string firstName)
        {
            var firstNameField = WaitForElement(By.Id("Input_FirstName"));
            _driver.FindElement(By.Id("Input_FirstName")).SendKeys(firstName);
        }

        public void FillLastName(string lastName)
        {
            _driver.FindElement(By.Id("Input_LastName")).SendKeys(lastName);
        }

        public void FillEmail(string email)
        {
            _driver.FindElement(By.Id("Input_Email")).SendKeys(email);
        }

        public void FillPassword(string password)
        {
            _driver.FindElement(By.Id("Input_Password")).SendKeys(password);
        }

        public void FillConfirmPassword(string confirmPassword)
        {
            _driver.FindElement(By.Id("Input_ConfirmPassword")).SendKeys(confirmPassword);
        }

        public void Submit()
        {
            _driver.FindElement(By.Id("register-submit")).Click();
        }
    }
}