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
        private IWebElement WaitForElement(By by)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            return wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    var displayed = element.Displayed;
                    var enabled = element.Enabled;
                    return (displayed && enabled) ? element : null;
                }
                catch (StaleElementReferenceException)
                {
                    return null; 
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
            });
        }

        public void Login(string email, string password)
        {
            var emailField = WaitForElement(By.Id("Input_Email"));
            emailField.Clear();
            emailField.SendKeys(email);

            var passwordField = WaitForElement(By.Id("Input_Password"));
            passwordField.Clear();
            passwordField.SendKeys(password);

            var loginButton = WaitForElement(By.Id("login-submit"));
            loginButton.Click();

            WaitForElement(By.Id("navbar-primary"));
        }
    }
}
