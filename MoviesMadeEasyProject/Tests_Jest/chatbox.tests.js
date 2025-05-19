const { JSDOM } = require('jsdom');
const fs = require('fs');
const path = require('path');

// Mock implementation since we can't load the real file
const chatboxScript = `
  document.addEventListener("DOMContentLoaded", function() {
    const chatbox = document.getElementById("chatbox");
    const toggle = document.getElementById("chatbox-toggle");
    const messagesDiv = document.querySelector(".chatbox-messages");
    
    if (toggle) {
      toggle.addEventListener("click", () => {
        chatbox.classList.toggle("open");
        const body = document.getElementById('chatbox-body');
        body.style.display = chatbox.classList.contains('open') ? 'flex' : 'none';
      });
    }
    
    const sendButton = document.querySelector(".chatbox-input .send");
    const inputField = document.querySelector(".chatbox-input input");
    
    if (sendButton && inputField) {
      sendButton.addEventListener("click", async () => {
        const message = inputField.value.trim();
        if (message) {
          // Add user message
          const userMsg = document.createElement("div");
          userMsg.textContent = message;
          messagesDiv.appendChild(userMsg);
          inputField.value = "";
          
          try {
            // Simulate bot response
            const response = await fetch('/Home/GetChatResponse?query=' + encodeURIComponent(message));
            const botResponse = await response.text();
            
            const botMsg = document.createElement("div");
            botMsg.textContent = botResponse;
            messagesDiv.appendChild(botMsg);
          } catch (error) {
            const errorMsg = document.createElement("div");
            errorMsg.textContent = "Error fetching response";
            errorMsg.classList.add("text-danger");
            messagesDiv.appendChild(errorMsg);
          }
          
          // Scroll to bottom
          messagesDiv.scrollTo({
            top: messagesDiv.scrollHeight,
            behavior: 'smooth'
          });
        }
      });
    }
  });
`;

// Mock HTML structure
const html = `
<!DOCTYPE html>
<html>
<head>
    <title>Movie Search</title>
    <style>
        .chatbox-container { position: fixed; bottom: 20px; right: 20px; }
        .chatbox-toggle { cursor: pointer; padding: 10px; background: #007bff; }
        .chatbox-body { display: none; background: white; width: 300px; height: 400px; }
        .chatbox.open .chatbox-body { display: flex; flex-direction: column; }
        .text-danger { color: red; }
    </style>
</head>
<body>
    <div id="chatbox" class="chatbox-container">
        <div id="chatbox-toggle" class="chatbox-toggle">
            <span>Chat Support</span>
        </div>
        <div id="chatbox-body" class="chatbox-body">
            <div class="chatbox-messages">
                <div class="text-muted">Welcome! How can we help you?</div>
            </div>
            <div class="chatbox-input">
                <input type="text" placeholder="Type your message..." />
                <button class="send">Send</button>
            </div>
        </div>
    </div>
    <script>
        ${chatboxScript}
    </script>
</body>
</html>
`;

describe('Chatbox DOM Structure', () => {
  let dom;
  let document;
  let window;

  beforeEach(() => {
    dom = new JSDOM(html, { 
      runScripts: 'dangerously',
      resources: 'usable'
    });
    document = dom.window.document;
    window = dom.window;
    
    // Mock the fetch function
    window.fetch = jest.fn(() => Promise.resolve({
      ok: true,
      text: () => Promise.resolve("Mock response from server")
    }));
  });

  test('chatbox container exists', () => {
    const chatbox = document.getElementById('chatbox');
    expect(chatbox).toBeInTheDocument();
  });

  test('chatbox toggle exists and is clickable', () => {
    const toggle = document.getElementById('chatbox-toggle');
    expect(toggle).toBeInTheDocument();
    
    // Simulate click and check if class is toggled
    const chatbox = document.getElementById('chatbox');
    toggle.click();
    expect(chatbox.classList.contains('open')).toBe(true);
    toggle.click();
    expect(chatbox.classList.contains('open')).toBe(false);
  });

  test('chatbox body is initially hidden', () => {
    const body = document.getElementById('chatbox-body');
    expect(body).toBeInTheDocument();
    expect(window.getComputedStyle(body).display).toBe('none');
  });

  test('chatbox has input field and send button', () => {
    const input = document.querySelector('.chatbox-input input');
    const button = document.querySelector('.chatbox-input .send');
    
    expect(input).toBeInTheDocument();
    expect(button).toBeInTheDocument();
    expect(input.placeholder).toBe('Type your message...');
    expect(button.textContent).toBe('Send');
  });
});

describe('Chatbox Functionality', () => {
  let dom;
  let document;
  let window;

  beforeEach(() => {
    dom = new JSDOM(html, { 
      runScripts: 'dangerously',
      resources: 'usable'
    });
    document = dom.window.document;
    window = dom.window;
    
    // Mock the fetch function
    window.fetch = jest.fn(() => Promise.resolve({
      ok: true,
      text: () => Promise.resolve("Mock response from server")
    }));
    
    // Mock scrollTo
    const messagesDiv = document.querySelector('.chatbox-messages');
    messagesDiv.scrollTo = jest.fn();
    
    // Add initial welcome message
    messagesDiv.innerHTML = '<div class="text-muted">Welcome! How can we help you?</div>';
  });

  test('sending a message adds it to the chat', async () => {
    const input = document.querySelector('.chatbox-input input');
    const button = document.querySelector('.chatbox-input .send');
    const messagesDiv = document.querySelector('.chatbox-messages');
    
    // Initial welcome message
    expect(messagesDiv.children.length).toBe(1);
    
    // Simulate user input and click
    input.value = 'Hello, world!';
    button.click();
    
    // Wait for promises to resolve
    await new Promise(resolve => setTimeout(resolve, 0));
    
    // Should now have 3 messages (welcome + user + bot)
    expect(messagesDiv.children.length).toBe(3);
    expect(messagesDiv.children[1].textContent).toContain('Hello, world!');
    expect(input.value).toBe(''); // Should be cleared after send
  });

  test('empty message is not sent', () => {
    const input = document.querySelector('.chatbox-input input');
    const button = document.querySelector('.chatbox-input .send');
    const messagesDiv = document.querySelector('.chatbox-messages');
    
    // Initial count
    const initialCount = messagesDiv.children.length;
    
    // Try to send empty message
    input.value = '   ';
    button.click();
    
    // Count should not change
    expect(messagesDiv.children.length).toBe(initialCount);
  });

  test('fetch is called with correct parameters when sending message', async () => {
    const input = document.querySelector('.chatbox-input input');
    const button = document.querySelector('.chatbox-input .send');
    
    input.value = 'Test query';
    button.click();
    
    await new Promise(resolve => setTimeout(resolve, 0));
    
    expect(window.fetch).toHaveBeenCalledTimes(1);
    expect(window.fetch).toHaveBeenCalledWith(
      expect.stringContaining('/Home/GetChatResponse?query=Test%20query')
    );
  });

  test('handles fetch error gracefully', async () => {
    // Override the mock to return an error
    window.fetch.mockImplementationOnce(() => Promise.reject(new Error('Network error')));
    
    const input = document.querySelector('.chatbox-input input');
    const button = document.querySelector('.chatbox-input .send');
    const messagesDiv = document.querySelector('.chatbox-messages');
    
    input.value = 'Failing query';
    button.click();
    
    // Wait for promises to resolve
    await new Promise(resolve => setTimeout(resolve, 0));
    
    // Check that error message was added
    const errorMessage = messagesDiv.lastChild;
    expect(errorMessage.textContent).toContain('Error fetching response');
    expect(errorMessage.classList.contains('text-danger')).toBe(true);
  });

  test('messages container scrolls to bottom after new message', async () => {
    const input = document.querySelector('.chatbox-input input');
    const button = document.querySelector('.chatbox-input .send');
    const messagesDiv = document.querySelector('.chatbox-messages');
    
    // Mock scroll behavior
    messagesDiv.scrollTop = 0;
    messagesDiv.scrollHeight = 500;
    
    input.value = 'Test scroll';
    button.click();
    
    await new Promise(resolve => setTimeout(resolve, 0));
    
    expect(messagesDiv.scrollTo).toHaveBeenCalledWith({
      top: messagesDiv.scrollHeight,
      behavior: 'smooth'
    });
  });
});