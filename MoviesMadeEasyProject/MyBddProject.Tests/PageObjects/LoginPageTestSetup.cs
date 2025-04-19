using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.Tests.PageObjects
{
    public class LoginPageTestSetup
    {
        private readonly IWebDriver _driver;

        public LoginPageTestSetup(IWebDriver driver)
        {
            _driver = driver;
        }

        public void Login(string email, string password)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var emailField = wait.Until(driver =>
            {
                var element = driver.FindElement(By.Id("Input_Email"));
                return element.Displayed && element.Enabled ? element : null;
            });

            emailField.SendKeys(email);

            _driver.FindElement(By.Id("Input_Password")).SendKeys(password);
            _driver.FindElement(By.Id("login-submit")).Click();
        }
    }
}
