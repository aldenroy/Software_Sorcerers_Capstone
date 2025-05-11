using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MyBddProject.PageObjects
{
    public class ChatboxPage
    {
        private readonly IWebDriver _driver;

        public ChatboxPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // Elements
        private IWebElement Chatbox => _driver.FindElement(By.Id("chatbox"));
        private IWebElement ChatboxBody => _driver.FindElement(By.Id("chatbox-body"));
        private IWebElement Chevron => _driver.FindElement(By.Id("chatbox-chevron"));
        private IWebElement InputField => _driver.FindElement(By.CssSelector(".chatbox-input input"));
        private IWebElement SendButton => _driver.FindElement(By.CssSelector(".chatbox-input .send"));
        private IWebElement WelcomeMessage => _driver.FindElement(By.CssSelector(".chatbox-messages div:first-child"));

        // Methods
        public void ClickChevron()
        {
            Chevron.Click();
        }

        public void ToggleChatbox()
        {
            _driver.FindElement(By.Id("chatbox-toggle")).Click();
        }

        public bool IsChatboxOpen()
        {
            return Chatbox.GetAttribute("class").Contains("open");
        }

        public bool IsChatboxBodyVisible()
        {
            return ChatboxBody.Displayed;
        }

        public string GetWelcomeMessage()
        {
            return WelcomeMessage.Text;
        }

        public void EnterMessage(string message)
        {
            InputField.SendKeys(message);
        }

        public void ClickSend()
        {
            SendButton.Click();
        }

        public int GetMessageCount()
        {
            return _driver.FindElements(By.CssSelector(".chatbox-messages div")).Count;
        }
    }
}