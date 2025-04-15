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

        public void FillFirstName(string firstName)
        {
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