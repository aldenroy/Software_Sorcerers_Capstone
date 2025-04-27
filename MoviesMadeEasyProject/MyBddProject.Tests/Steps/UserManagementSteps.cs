using System;
using OpenQA.Selenium;
using Reqnroll;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.Tests.PageObjects;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class UserManagementSteps
    {
        private readonly IWebDriver _driver;
        private readonly LoginPageTestSetup _loginPage;
        private readonly RegistrationPageTestSetup _registrationPage;

        public UserManagementSteps(IWebDriver driver)
        {
            _driver = driver;
            _loginPage = new LoginPageTestSetup(_driver);
            _registrationPage = new RegistrationPageTestSetup(_driver);
        }

        [BeforeScenario]
        public void SetupImplicitWait()
        {
            try
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set implicit wait: {ex.Message}");
            }
        }

        private bool IsElementPresent(By by)
        {
            try
            {
                return _driver.FindElement(by).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private void DeleteUserAccount()
        {
            try
            {
                if (IsLoggedIn())
                {
                    Console.WriteLine("Cleaning up test user account");
                    _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Manage/DeletePersonalData");

                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

                    if (_driver.Url.Contains("DeletePersonalData"))
                    {
                        wait.Until(d => d.FindElement(By.Id("Input_Password")));

                        var passwordInput = _driver.FindElement(By.Id("Input_Password"));
                        passwordInput.SendKeys("Test!123");

                        var deleteButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                        deleteButton.Click();

                        wait.Until(d => d.Url.Contains("http://localhost:5000/"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user account: {ex.Message}");
            }
        }

        private bool IsLoggedIn()
        {
            try
            {
                return IsElementPresent(By.LinkText("Logout"));
            }
            catch
            {
                return false;
            }
        }

        [Given(@"a user with the email ""(.*)"" exists in the system")]
        public void GivenAUserWithTheEmailExistsInTheSystem(string email)
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Register");

            try
            {
                _registrationPage.FillFirstName("Test");
                _registrationPage.FillLastName("User");
                _registrationPage.FillEmail(email);
                _registrationPage.FillPassword("Test!123");
                _registrationPage.FillConfirmPassword("Test!123");
                _registrationPage.Submit();

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.Url.Contains("/Dashboard") || IsElementPresent(By.CssSelector(".validation-summary-errors")));

                if (IsElementPresent(By.LinkText("Logout")))
                {
                    _driver.FindElement(By.LinkText("Logout")).Click();
                    wait.Until(d => d.Url.Contains("http://localhost:5000/"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Given Step] Error during user setup: {ex.Message}");
            }
        }

        [Given(@"the user is on the login page")]
        public void GivenTheUserIsOnTheLoginPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("Input_Email")).Displayed);
        }

        private string _email;
        private string _password;

        [When(@"the user enters ""(.*)"" in the email field")]
        public void WhenTheUserEntersInTheEmailField(string email)
        {
            _email = email;
        }

        [When(@"the user enters ""(.*)"" in the password field")]
        public void WhenTheUserEntersInThePasswordField(string password)
        {
            _password = password;
            _loginPage.Login(_email, _password);
        }

        [Then(@"the user should see an error message")]
        public void ThenTheUserShouldSeeAnErrorMessage()
        {
            var errorMessage = _driver.FindElement(By.CssSelector(".validation-summary-errors ul li"));
            Assert.That(errorMessage.Text, Is.EqualTo("Invalid login attempt."), "Error message is not as expected.");
        }

        [Then(@"the user should remain on the login page")]
        public void ThenTheUserShouldRemainOnTheLoginPage()
        {
            Assert.That(_driver.Url, Is.EqualTo("http://localhost:5000/Identity/Account/Login"));
        }

        [Then(@"the user will be logged in and redirected to the dashboard page")]
        public void ThenTheUserWillBeLoggedInAndRedirectedToTheDashboardPage()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => driver.Url.Contains("/Dashboard"));
            Assert.That(_driver.Url, Does.Contain("/Dashboard"));
        }

        [Given(@"the user is on the registration page")]
        public void GivenTheUserIsOnTheRegistrationPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Register");

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("Input_Email")).Displayed);
        }

        [When(@"the user enters ""(.*)"" in the first name field")]
        public void WhenTheUserEntersInTheFirstNameField(string firstName)
        {
            _registrationPage.FillFirstName(firstName);
        }

        [When(@"the user enters ""(.*)"" in the last name field")]
        public void WhenTheUserEntersInTheLastNameField(string lastName)
        {
            _registrationPage.FillLastName(lastName);
        }

        [When(@"the user enters ""(.*)"" in the registration email field")]
        public void WhenTheUserEntersInTheRegistrationEmailField(string email)
        {
            _registrationPage.FillEmail(email);
        }

        [When(@"the user enters ""(.*)"" in the registration password field")]
        public void WhenTheUserEntersInTheRegistrationPasswordField(string password)
        {
            _registrationPage.FillPassword(password);
        }

        [When(@"the user enters ""(.*)"" in the password confirmation field")]
        public void WhenTheUserEntersInThePasswordConfirmationField(string confirmPassword)
        {
            _registrationPage.FillConfirmPassword(confirmPassword);
        }

        [When(@"the user submits the form")]
        public void WhenTheUserSubmitsTheForm()
        {
            _registrationPage.Submit();
        }

        [Then(@"the user should be redirected to the preferences page")]
        public void ThenTheUserShouldBeRedirectedToThePreferencesPage()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => driver.Url.Contains("/Preferences"));
            Assert.That(_driver.Url, Does.Contain("/Preferences"));

            // Cleanup the user account immediately after the successful registration test
            DeleteUserAccount();
        }

        [Then(@"the user should see an error message for the duplicate email")]
        public void ThenTheUserShouldSeeAnErrorMessageForTheDuplicateEmail()
        {
            var validationSummary = _driver.FindElement(By.CssSelector(".validation-summary-errors ul li"));
            Assert.That(validationSummary.Text, Is.EqualTo("Username 'test@test.com' is already taken."), "Error message is not as expected.");
        }

        [Then(@"the user should remain on the registration page")]
        public void ThenTheUserShouldRemainOnTheRegistrationPage()
        {
            Assert.That(_driver.Url, Is.EqualTo("http://localhost:5000/Identity/Account/Register"));
        }
    }
}