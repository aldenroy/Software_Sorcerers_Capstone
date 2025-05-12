using Reqnroll;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using MyBddProject.PageObjects;
using System;

namespace MyBddProject.Tests.Steps
{
    [Binding]
    public class ChatboxSteps
    {
        private readonly IWebDriver _driver;
        private readonly SearchPage _searchPage;
        private readonly ChatboxPage _chatboxPage;

        public ChatboxSteps(IWebDriver driver)
        {
            _driver = driver;
            _searchPage = new SearchPage(driver);
            _chatboxPage = new ChatboxPage(driver);
        }

        [Given(@"the user is on the main page")]
        public void GivenTheUserIsOnTheSearchPage()
        {
            if (!_driver.Url.Contains("localhost:5000"))
            {
                _driver.Navigate().GoToUrl("http://localhost:5000/");
            }
            
            Assert.IsTrue(_driver.PageSource.Contains("Movie Search"));
            Assert.IsTrue(_searchPage.IsSearchInputDisplayed());
        }

        [When(@"the user clicks the up chevron")]
        public void WhenTheUserClicksTheUpChevron()
        {
            _chatboxPage.ClickChevron();
        }

        [Then(@"the chatbox should pop out")]
        public void ThenTheChatboxShouldPopOut()
        {
            Assert.IsTrue(_chatboxPage.IsChatboxOpen());
            Assert.IsTrue(_chatboxPage.IsChatboxBodyVisible());
        }

        [Given(@"the user has opened the chatbox")]
        public void GivenTheUserHasOpenedTheChatbox()
        {
            _chatboxPage.ToggleChatbox();
            Assert.IsTrue(_chatboxPage.IsChatboxOpen());
        }

        [When(@"the user sees the chatbox")]
        public void WhenTheUserSeesTheChatbox()
        {
            // Verification happens in the Then step
        }

        [Then(@"the user should see a welcome message")]
        public void ThenTheUserShouldSeeAWelcomeMessage()
        {
            Assert.IsTrue(_chatboxPage.GetWelcomeMessage().Contains("Welcome! How can we help you?"));
        }

        [Given(@"the user is in the chatbox")]
        public void GivenTheUserHasOpenedTheChatboxTwo()
        {
            _chatboxPage.ToggleChatbox();
            Assert.IsTrue(_chatboxPage.IsChatboxOpen());
        }

        [When(@"the user types a message into the textbox")]
        public void WhenTheUserTypesAMessageIntoTheTextbox()
        {
            _chatboxPage.EnterMessage("How do I search for movies?");
        }

        [Then(@"the user should see a reply message")]
        public void ThenTheUserShouldSeeAReplyMessage()
        {
            _chatboxPage.ClickSend();
            
            // Wait for response
            new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
                .Until(d => _chatboxPage.GetMessageCount() >= 2);
            
            Assert.GreaterOrEqual(_chatboxPage.GetMessageCount(), 2);
        }

        [Given(@"the user is on the open chatbox")]
        public void GivenTheUserIsOnTheOpenChatbox()
        {
            _chatboxPage.ToggleChatbox();
            Assert.IsTrue(_chatboxPage.IsChatboxOpen());
        }

        [When(@"the user clicks the down chevron")]
        public void WhenTheUserClicksTheDownChevron()
        {
            _chatboxPage.ClickChevron();
        }

        [Then(@"the chatbox should pop down")]
        public void ThenTheChatboxShouldPopDown()
        {
            Assert.IsFalse(_chatboxPage.IsChatboxOpen());
            Assert.IsFalse(_chatboxPage.IsChatboxBodyVisible());
        }
    }
}