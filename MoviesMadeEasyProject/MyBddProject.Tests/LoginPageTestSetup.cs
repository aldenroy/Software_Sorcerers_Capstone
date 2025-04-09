using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.Tests
{
    public class LoginPageTestSetup
    {
        private readonly IWebDriver _driver;

        public LoginPageTestSetup(IWebDriver driver)
        {
            _driver = driver;
        }

        public void GoTo()
        {
            _driver.Navigate().GoToUrl("http://localhost:5260/Identity/Account/Login");
        }

        public void Login(string email, string password)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            _driver.FindElement(By.Id("Input_Email")).SendKeys(email);
            _driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            _driver.FindElement(By.Id("login-submit")).Click();
        }
    }
}
