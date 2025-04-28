using System;
using Reqnroll;
using OpenQA.Selenium;
using MyBddProject.Tests.Steps;
using MyBddProject.Tests.PageObjects;

namespace MyNamespace.Steps
{
    [Binding]
    public class RecentViewedTitlesSteps
    {
        private readonly DashboardSteps _dashboardSteps;
        private readonly IWebDriver _driver;

        public RecentViewedTitlesSteps(DashboardSteps dashboardSteps, IWebDriver driver)
        {
            _dashboardSteps = dashboardSteps;
            _driver = driver;
        }

        [Given("I have viewed \"(.*)\" movie")]
        public void GivenIHaveViewed(string movieTitle)
        {
            var titles = _driver.FindElements(By.CssSelector(".movie-title"));
            bool movieSeeded = titles.Any(e => e.Text.Trim().Equals(movieTitle, StringComparison.OrdinalIgnoreCase));

            if (!movieSeeded)
            {
                throw new Exception($"Expected movie '{movieTitle}' to be seeded but it was not found on the dashboard.");
            }
        }

        [Given(@"I see a ""(.*)"" section listing the movies, including ""(.*)""")]
        [Then("I see a \"(.*)\" section listing the movies, including \"(.*)\"")]
        public void ThenISeeASectionListingTheMoviesIncluding(string section, string movieTitle)
        {
            var carousel = _driver.FindElement(By.Id("recentlyViewedCarousel"));
            if (carousel == null)
                throw new Exception("Could not find the Recently Viewed carousel.");

            var movieTitles = carousel.FindElements(By.CssSelector(".movie-title"));

            bool movieFound = movieTitles
                .Any(e => e.Text.Trim().Equals(movieTitle, StringComparison.OrdinalIgnoreCase));

            if (!movieFound)
            {
                throw new Exception($"Movie '{movieTitle}' not found in the Recently Viewed carousel.");
            }
        }

        [Given("I have viewed \"(.*)\" and then \"(.*)\"")]
        public void GivenIHaveViewedAndThen(string firstMovie, string secondMovie)
        {
            var titles = _driver.FindElements(By.CssSelector(".movie-title"));

            bool firstMovieFound = titles.Any(e => e.Text.Trim().Equals(firstMovie, StringComparison.OrdinalIgnoreCase));
            bool secondMovieFound = titles.Any(e => e.Text.Trim().Equals(secondMovie, StringComparison.OrdinalIgnoreCase));

            if (!firstMovieFound || !secondMovieFound)
            {
                throw new Exception($"Expected movies '{firstMovie}' and '{secondMovie}' to be seeded but they were not both found on the dashboard.");
            }
        }

        [Then("I see a \"(.*)\" section listing the movies with the most recently viewed on the left, so \"(.*)\" appears to the left of \"(.*)\"")]
        public void ThenISeeASectionListingTheMoviesWithTheMostRecentlyViewedOnTheLeft(string section, string firstMovie, string secondMovie)
        {
            var carousel = _driver.FindElement(By.Id("recentlyViewedCarousel"));
            if (carousel == null)
            {
                throw new Exception("Could not find the Recently Viewed carousel.");
            }

            var movieTitles = carousel.FindElements(By.CssSelector(".movie-title"))
                                       .Select(e => e.Text.Trim())
                                       .ToList();

            int firstIndex = movieTitles.IndexOf(firstMovie);
            int secondIndex = movieTitles.IndexOf(secondMovie);

            if (firstIndex == -1)
            {
                throw new Exception($"Movie '{firstMovie}' not found in Recently Viewed carousel.");
            }

            if (secondIndex == -1)
            {
                throw new Exception($"Movie '{secondMovie}' not found in Recently Viewed carousel.");
            }

            if (firstIndex >= secondIndex)
            {
                throw new Exception($"Movie '{firstMovie}' does not appear before '{secondMovie}' in Recently Viewed carousel.");
            }
        }

        [Then("a screen reader reads the \"(.*)\" section")]
        public void ThenAScreenReaderReadsTheSection(string section)
        {
            var sectionElement = _driver.FindElement(By.Id("recentlyViewedCarousel"));
            if (sectionElement == null)
                throw new Exception($"Could not find the '{section}' section.");

            var ariaLabelledBy = sectionElement.GetAttribute("aria-labelledby");
            if (string.IsNullOrWhiteSpace(ariaLabelledBy))
                throw new Exception($"'{section}' section is missing 'aria-labelledby' attribute for screen readers.");

            var heading = _driver.FindElement(By.Id(ariaLabelledBy));
            if (heading == null)
                throw new Exception($"Could not find heading element with id '{ariaLabelledBy}'.");

            var headingText = heading.Text.Trim();
            if (!headingText.Contains(section, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Screen reader heading text '{headingText}' does not match expected section '{section}'.");
        }

        [Then("I hear descriptive labels for each movie that include the movie title")]
        public void ThenIHearDescriptiveLabelsForEachMovieThatIncludeTheMovieTitle()
        {
            var movieLinks = _driver.FindElements(By.CssSelector("a[aria-label*='view details']"));
            if (!movieLinks.Any())
                throw new Exception("No movie links found with descriptive labels.");

            foreach (var link in movieLinks)
            {
                var ariaLabel = link.GetAttribute("aria-label");
                var movieTitleElement = link.FindElement(By.CssSelector(".movie-title"));
                var movieTitle = movieTitleElement.Text.Trim();

                if (string.IsNullOrWhiteSpace(ariaLabel) || !ariaLabel.Contains(movieTitle, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"Movie '{movieTitle}' does not have a descriptive aria-label that includes its title.");
                }
            }
        }

        [Given("I am logged in as a new user with no viewed movies")]
        public void GivenIAmLoggedInAsNewUserWithNoViewedMovies()
        {
            var loginPage = new LoginPageTestSetup(_driver);
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");
            loginPage.Login("testuser2@example.com", "Ab+1234");
        }

        [Then("I see the message \"(.*)\" announced as a status message")]
        public void ThenISeeTheMessageAnnouncedAsStatusMessage(string message)
        {
            var noTitlesElement = _driver.FindElement(By.Id("no-recent-titles"));
            if (noTitlesElement == null)
                throw new Exception("No element with id 'no-recent-titles' found.");

            var roleAttribute = noTitlesElement.GetAttribute("role");
            if (!string.Equals(roleAttribute, "status", StringComparison.OrdinalIgnoreCase))
                throw new Exception("No 'status' role found on the no-titles message element.");

            var actualMessage = noTitlesElement.Text.Trim();
            if (!actualMessage.Equals(message, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Expected status message '{message}', but found '{actualMessage}'.");
        }

        [When("I navigate through the movies using keyboard commands \\(such as Tab or arrow keys)")]
        public void WhenINavigateThroughTheMoviesUsingKeyboardCommands()
        {
            var body = _driver.FindElement(By.TagName("body"));

            for (int i = 0; i < 20; i++)
            {
                body.SendKeys(Keys.Tab);
                var focused = _driver.SwitchTo().ActiveElement();

                if (focused.TagName.Equals("a", StringComparison.OrdinalIgnoreCase))
                {
                    var ariaLabel = focused.GetAttribute("aria-label");
                    if (!string.IsNullOrWhiteSpace(ariaLabel) &&
                        ariaLabel.Contains("view details", StringComparison.OrdinalIgnoreCase))
                    {
                        return; 
                    }
                }
            }

            throw new Exception("Could not navigate to a movie link using keyboard Tab.");
        }

        [Then("I can select and activate a movie using keyboard only, without requiring a mouse")]
        public void ThenICanSelectAndActivateAMovieUsingKeyboardOnly()
        {
            var focusedElement = _driver.SwitchTo().ActiveElement();

            if (!focusedElement.TagName.Equals("a", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Focused element is not a movie link (expected 'a' tag).");
            }

            var ariaLabel = focusedElement.GetAttribute("aria-label");
            if (string.IsNullOrWhiteSpace(ariaLabel) ||
                !ariaLabel.Contains("view details", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Focused movie link does not have a valid descriptive aria-label.");
            }

            focusedElement.SendKeys(Keys.Enter);
        }

    }
}
