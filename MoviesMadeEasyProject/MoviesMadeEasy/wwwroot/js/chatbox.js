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

                const userMessage = document.createElement("div");
                userMessage.classList.add("text-end", "my-2");
                userMessage.innerHTML = `<span class="chat-bubble user-message border">${message}</span>`;
                messagesDiv.appendChild(userMessage);
                inputField.value = "";
                messagesDiv.scrollTop = messagesDiv.scrollHeight;

                try {
                    const response = await fetch(`/Home/GetChatResponse?query=${encodeURIComponent(message)}`);
                    if (!response.ok) throw new Error("Network response was not ok");
                    const data = await response.text();

                    const botMessage = document.createElement("div");
                    botMessage.classList.add("chat-bubble", "bot-message", "border");

                    const messageContent = document.createElement("span");
                    messageContent.classList.add("chat-bubble", "bot-message", "border");
                    messageContent.innerHTML = data;

                    messageContent.querySelectorAll('.quoted-link').forEach(link => {
                        link.addEventListener('click', async (e) => {
                            e.preventDefault();
                            const movieTitle = link.dataset.movie;
                            console.log("Movie selected:", movieTitle);
                            try {
                                await getMovie(movieTitle);
                                showMovieModal();
                            } catch (error) {
                                console.error("Error fetching movie:", error);
                                // Show error in chat
                                const errorMsg = document.createElement("div");
                                errorMsg.classList.add("text-start", "my-2", "text-danger");
                                errorMsg.textContent = "Error fetching movie details.";
                                messagesDiv.appendChild(errorMsg);
                            }
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

async function getMovie(movieTitle) {
    let queryParams = new URLSearchParams({ query: movieTitle });
    let response = await fetch(`/Home/SearchMovies?${queryParams.toString()}`);
    let movieData = await response.json();

    if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Request failed');
    }

    // Store the first movie result
    sessionStorage.setItem('movieData', JSON.stringify(movieData[0]));
    sessionStorage.setItem('title', movieTitle);
}

// Update the showMovieModal function in chatbox.js
function showMovieModal() {
    const movieDataRaw = sessionStorage.getItem('movieData');
    const title = sessionStorage.getItem('title');
    const streamingServices = {
        'Netflix': {
            logo: '/images/Netflix_Symbol_RGB.png',
            link: 'https://www.netflix.com/login'
        },
        'Hulu': {
            logo: '/images/hulu-Green-digital.png',
            link: 'https://auth.hulu.com/web/login'
        },
        'Disney+': {
            logo: '/images/disney_logo_march_2024_050fef2e.png',
            link: 'https://www.disneyplus.com/login'
        },
        'Prime Video': {
            logo: '/images/AmazonPrimeVideo.png',
            link: 'https://www.primevideo.com'
        },
        'Max': {
            logo: '/images/maxlogo.jpg',
            link: 'https://play.max.com/sign-in'
        },
        'Apple TV': {
            logo: '/images/AppleTV-iOS.png',
            link: 'https://tv.apple.com/login'
        },
        'Peacock': {
            logo: '/images/Peacock_P.png',
            link: 'https://www.peacocktv.com/signin'
        },
        'Paramount+': {
            logo: '/images/Paramountplus.png',
            link: 'https://www.paramountplus.com/account/signin/'
        },
        'Starz': {
            logo: '/images/Starz_Prism_Button_Option_01.png',
            link: 'https://www.starz.com/login'
        },
        'Tubi': {
            logo: '/images/tubitvlogo.png',
            link: 'https://tubitv.com/login'
        },
        'Pluto TV': {
            logo: '/images/Pluto-TV-Logo.jpg',
            link: 'https://pluto.tv/en/login'
        },
        'BritBox': {
            logo: '/images/britboxlogo.png',
            link: 'https://www.britbox.com/us/'
        },
        'AMC+': {
            logo: '/images/amcpluslogo.png',
            link: 'https://www.amcplus.com/login'
        }
    };

    if (!movieDataRaw) {
        console.error("No movie data found in session storage");
        return;
    }

    try {
        const movie = JSON.parse(movieDataRaw);
        const modal = new bootstrap.Modal(document.getElementById('movieModal'));
        
        // Get all modal elements
        const modalTitle = document.getElementById('modalTitle');
        const modalPoster = document.getElementById('modalPoster');
        const modalGenres = document.getElementById('modalGenres');
        const modalRating = document.getElementById('modalRating');
        const modalOverview = document.getElementById('modalOverview');
        const modalStreaming = document.getElementById('modalStreaming');

        // Set modal title
        modalTitle.textContent = title || movie.title || "Movie Info";

        // Set poster image if available
        if (movie.poster_path) {
            modalPoster.src = `https://image.tmdb.org/t/p/w500${movie.poster_path}`;
            modalPoster.style.display = 'block';
            modalPoster.alt = `${title} movie poster`;
        } else if (movie.posterUrl) {
            modalPoster.src = movie.posterUrl;
            modalPoster.style.display = 'block';
            modalPoster.alt = `${title} movie poster`;
        } else {
            modalPoster.style.display = 'none';
        }

        // Set genres
        if (movie.genres && movie.genres.length > 0) {
            // Check if genres is an array of objects or strings
            const genreNames = typeof movie.genres[0] === 'object' 
                ? movie.genres.map(g => g.name) 
                : movie.genres;
            modalGenres.innerHTML = `<strong>Genres:</strong> ${genreNames.join(', ')}`;
        } else if (movie.genre) {
            modalGenres.innerHTML = `<strong>Genres:</strong> ${movie.genre}`;
        } else {
            modalGenres.innerHTML = '<strong>Genres:</strong> Not available';
        }

        // Set rating
        if (movie.vote_average) {
            const voteCount = movie.vote_count ? `(${movie.vote_count} votes)` : '';
            modalRating.innerHTML = `<strong>Rating:</strong> ${movie.vote_average}/10 ${voteCount}`;
        } else if (movie.rating) {
            modalRating.innerHTML = `<strong>Rating:</strong> ${movie.rating}`;
        } else {
            modalRating.innerHTML = '<strong>Rating:</strong> Not available';
        }

        // Set overview
        modalOverview.textContent = movie.overview || movie.description || "No overview available.";

        // Set streaming services - UPDATED VERSION WITH CLICKABLE LINKS
        modalStreaming.innerHTML = '';
        const servicesToShow = movie.streaming_services || movie.services || [];
        const addedServices = new Set(); // To avoid duplicates
        
        if (servicesToShow.length > 0) {
            servicesToShow.forEach(service => {
                // Find the matching service name (case insensitive)
                const serviceKey = Object.keys(streamingServices).find(key => 
                    key.toLowerCase() === service.toLowerCase()
                );
                
                if (serviceKey && streamingServices[serviceKey] && !addedServices.has(serviceKey)) {
                    addedServices.add(serviceKey);
                    
                    const serviceInfo = streamingServices[serviceKey];
                    
                    // Create link element
                    const link = document.createElement('a');
                    link.href = serviceInfo.link;
                    link.target = '_blank';
                    link.rel = 'noopener noreferrer';
                    link.title = serviceKey;
                    
                    // Create image element
                    const img = document.createElement('img');
                    img.src = serviceInfo.logo;
                    img.alt = serviceKey;
                    img.style.height = '32px';
                    img.style.width = 'auto';
                    img.style.margin = '0 5px';
                    img.classList.add('streaming-icon');
                    
                    // Append image to link
                    link.appendChild(img);
                    
                    // Append link to container
                    modalStreaming.appendChild(link);
                }
            });
            
            if (modalStreaming.children.length === 0) {
                modalStreaming.innerHTML = '<p>No streaming information available</p>';
            }
        } else {
            modalStreaming.innerHTML = '<p>No streaming information available</p>';
        }

        // Show the modal
        modal.show();
    } catch (error) {
        console.error("Error parsing movie data:", error);
    }
}