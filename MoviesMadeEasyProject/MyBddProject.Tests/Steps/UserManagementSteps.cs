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
        private string _email;
        private string _password;

        public UserManagementSteps(IWebDriver driver)
        {
            _driver = driver;
            _loginPage = new LoginPageTestSetup(_driver);
            _registrationPage = new RegistrationPageTestSetup(_driver);
            _email = string.Empty;
            _password = string.Empty;
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
            _email = email;
            _registrationPage.FillEmail(email);
        }

        [When(@"the user enters ""(.*)"" in the registration password field")]
        public void WhenTheUserEntersInTheRegistrationPasswordField(string password)
        {
            _password = password;
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
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20)); // Extended timeout
                wait.Until(driver => driver.Url.Contains("/Preferences"));

                Assert.That(_driver.Url, Does.Contain("/Preferences"), "Not redirected to Preferences page");

                // Immediately delete this test account - crucial for test stability
                DeleteUserAccount(_email, _password);

                Console.WriteLine("Cleaned up test user account");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during preferences page check: {ex.Message}");

                // Even if the test fails, try to clean up
                DeleteUserAccount(_email, _password);

                throw;
            }
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

        private void DeleteUserAccount(string email, string password)
        {
            Console.WriteLine($"Attempting to delete user account: {email}");
            try
            {
                // First make sure we're logged in with the correct user
                _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

                // Wait for login page to load
                wait.Until(d => IsElementPresent(By.Id("Input_Email")));

                // Fill in login details
                var emailInput = _driver.FindElement(By.Id("Input_Email"));
                emailInput.Clear();
                emailInput.SendKeys(email);

                var passwordInput = _driver.FindElement(By.Id("Input_Password"));
                passwordInput.Clear();
                passwordInput.SendKeys(password);

                // Submit the form
                var loginButton = _driver.FindElement(By.Id("login-submit"));
                loginButton.Click();

                // Wait for redirect
                wait.Until(d => d.Url.Contains("Dashboard") || IsElementPresent(By.CssSelector(".validation-summary-errors")));

                // Only proceed if login was successful
                if (IsLoggedIn())
                {
                    Console.WriteLine("Successfully logged in for account deletion");

                    // Navigate to delete account page
                    _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Manage/DeletePersonalData");

                    // Wait for page to load
                    wait.Until(d => IsElementPresent(By.Id("Input_Password")));

                    // Confirm with password
                    var confirmPassword = _driver.FindElement(By.Id("Input_Password"));
                    confirmPassword.Clear();
                    confirmPassword.SendKeys(password);

                    // Click delete button
                    var deleteButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                    deleteButton.Click();

                    // Wait for redirect after deletion (should go back to home page)
                    wait.Until(d => d.Url.Equals("http://localhost:5000/") ||
                                   d.Url.Equals("http://localhost:5000/Identity/Account/Login"));

                    Console.WriteLine("Account deletion completed");
                }
                else
                {
                    Console.WriteLine("Warning: Could not log in to delete account");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during account deletion: {ex.Message}");
            }
        }
    }
}