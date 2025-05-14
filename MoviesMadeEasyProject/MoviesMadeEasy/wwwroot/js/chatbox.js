
document.addEventListener("DOMContentLoaded", function () {
    const chatbox = document.getElementById("chatbox");
    const toggle = document.getElementById("chatbox-toggle");
    const body = document.getElementById("chatbox-body");
    const close = document.getElementById("chatbox-close");
    const chevron = document.getElementById("chatbox-chevron");
    const sendButton = document.querySelector(".chatbox-input .send");
    const inputField = document.querySelector(".chatbox-input input");

    toggle.addEventListener("click", () => {
        chatbox.classList.toggle("open");
    });

    if (close) {
        close.addEventListener("click", () => {
            chatbox.classList.remove("open");
        });
    }

    if (sendButton && inputField) {
        sendButton.addEventListener("click", async () => {
            const message = inputField.value.trim();
            if (message !== "") {
                const messagesDiv = document.querySelector(".chatbox-messages");

                // Add user's message
                const userMessage = document.createElement("div");
                userMessage.classList.add("text-end", "my-2");
                userMessage.innerHTML = `<span class="bg-primary text-white p-2 rounded">${message}</span>`;
                messagesDiv.appendChild(userMessage);
                inputField.value = "";
                messagesDiv.scrollTop = messagesDiv.scrollHeight;

                // Send fetch request to backend
                try {
                    const response = await fetch(`/Home/GetChatResponse?query=${encodeURIComponent(message)}`);
                    if (!response.ok) throw new Error("Network response was not ok");
                    const data = await response.text();
                
                    // Create container for bot's response
                    const botMessage = document.createElement("div");
                    botMessage.classList.add("text-start", "my-2");
                    
                    // Create span for message content
                    const messageContent = document.createElement("span");
                    messageContent.classList.add("bg-light", "border", "p-2", "rounded", "d-inline-block");
                    
                    // Set the HTML content directly
                    messageContent.innerHTML = data;
                    
                    // Add click handlers to all quoted links
                    messageContent.querySelectorAll('.quoted-link').forEach(link => {
                        link.addEventListener('click', (e) => {
                            e.preventDefault();
                            const movieTitle = link.dataset.movie;
                            console.log("Movie selected:", movieTitle);
                            // Add your click handling logic here
                        });
                    });
                    
                    botMessage.appendChild(messageContent);
                    messagesDiv.appendChild(botMessage);
                    messagesDiv.scrollTop = messagesDiv.scrollHeight;
                } catch (error) {
                    const errorMsg = document.createElement("div");
                    errorMsg.classList.add("text-start", "my-2", "text-danger");
                    errorMsg.textContent = "Error fetching response.";
                    messagesDiv.appendChild(errorMsg);
                }
            }
        });
    }
});

