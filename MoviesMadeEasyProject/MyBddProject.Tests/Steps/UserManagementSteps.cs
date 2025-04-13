using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Reqnroll;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.Tests.PageObjects;

namespace MyProject.Tests.StepDefinitions
{
    [Binding]
    public class UserManagementSteps
    {
        private IWebDriver _driver;
        private LoginPageTestSetup _loginPage;
        private RegistrationPageTestSetup _registrationPage;

        [BeforeScenario]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _loginPage = new LoginPageTestSetup(_driver);
            _registrationPage = new RegistrationPageTestSetup(_driver);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
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

        [AfterScenario]
        public void TearDown()
        {
            if (_driver != null)
            {
                try
                {
                    _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Manage/DeletePersonalData");

                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => d.FindElement(By.Id("Input_Password")));

                    var passwordInput = _driver.FindElement(By.Id("Input_Password"));
                    passwordInput.SendKeys("Test!123");

                    var deleteButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                    deleteButton.Click();

                    wait.Until(d => d.Url.Contains("http://localhost:5000/"));

                    var homePageText = _driver.FindElement(By.CssSelector("h2")).Text;
                    Assert.That(homePageText, Is.EqualTo("MoviesMadeEasy"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TearDown error: {ex.Message}");
                }
                finally
                {
                    _driver.Quit();
                    _driver.Dispose();
                }
            }
        }

        [Given(@"a user with the email ""(.*)"" exists in the system")]
        public void GivenAUserWithTheEmailExistsInTheSystem(string email)
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Register");
            _registrationPage.FillFirstName("Test");
            _registrationPage.FillLastName("User");
            _registrationPage.FillEmail(email);
            _registrationPage.FillPassword("Test!123");
            _registrationPage.FillConfirmPassword("Test!123");
            _registrationPage.Submit();

            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                if (IsElementPresent(By.LinkText("Logout")))
                {
                    var logoutLink = wait.Until(d => d.FindElement(By.LinkText("Logout")));
                    logoutLink.Click();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Given Step] Error during logout: {ex.Message}");
            }
        }

        [Given(@"the user is on the login page")]
        public void GivenTheUserIsOnTheLoginPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");
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

        // Unsuccessful Login
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

        // Successful Login
        [Then(@"the user will be logged in and redirected to the dashboard page")]
        public void ThenTheUserWillBeLoggedInAndRedirectedToTheDashboardPage()
        {
            Assert.That(_driver.Url, Does.Contain("/Dashboard"));
        }

        [Given(@"the user is on the registration page")]
        public void GivenTheUserIsOnTheRegistrationPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Register");
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

        // Successful Registration
        [Then(@"the user should be redirected to the preferences page")]
        public void ThenTheUserShouldBeRedirectedToThePreferencesPage()
        {
            Assert.That(_driver.Url, Does.Contain("/Preferences"));
        }

        // Unsuccessful Registration Duplicate Email
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
