document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("results")?.addEventListener("click", function (event) {
        if (event.target.classList.contains("btn-primary")) {
            let movieCard = event.target.closest(".movie-card");

            let title = movieCard.querySelector("h5").textContent;
            let posterUrl = movieCard.querySelector("img").src;
            let genres = movieCard.querySelector(".movie-genres").textContent.replace("Genres: ", "");
            let rating = movieCard.querySelector(".movie-rating").textContent.replace("Rating: ", "");
            
            let overview = movieCard.getAttribute("data-overview") || "No overview available.";
            let streamingServices = movieCard.getAttribute("data-streaming") || "";

            // Populate the modal with movie details
            document.getElementById("modalTitle").textContent = title;
            document.getElementById("modalPoster").src = posterUrl;
            document.getElementById("modalGenres").textContent = `Genres: ${genres}`;
            document.getElementById("modalRating").textContent = `Rating: ${rating}`;
            document.getElementById("modalOverview").textContent = `Overview: ${overview}`;
            
            // Clear previous streaming icons
            const streamingContainer = document.getElementById("modalStreaming");
            streamingContainer.innerHTML = "";
            
            // Add streaming service icons if available
            if (streamingServices) {
                const services = streamingServices.split(",");
                console.log(services)
                services.forEach(service => {
                    const trimmedService = service.trim();
                    if (trimmedService) {
                        const link = document.createElement("a");
                        link.href = getStreamingServiceLink(trimmedService); // Use the function that returns links
                        link.target = "_blank"; // Open in new tab
                        link.rel = "noopener noreferrer"; // Security best practice
                        
                        const icon = document.createElement("img");
                        icon.src = getStreamingServiceLogo(trimmedService); // This should return the image URL
                        icon.alt = trimmedService;
                        icon.title = trimmedService;

                        icon.className = "streaming-icon";
                        icon.style.width = "40px";
                        icon.style.height = "40px";
                        icon.style.objectFit = "contain";
                        icon.style.marginRight = "5px";
                        
                        link.appendChild(icon);
                        streamingContainer.appendChild(link);
                    }
                });
            } else {
                streamingContainer.textContent = "Not available on streaming platforms.";
            }

            // Show the modal
            var movieModal = new bootstrap.Modal(document.getElementById("movieModal"));
            movieModal.show();
        }
    });
});

// Helper function to get logo URLs for streaming services
function getStreamingServiceLogo(serviceName) {
    const serviceLogos = {
        'Netflix': '/images/Netflix_Symbol_RGB.png',
        'Hulu': '/images/hulu-Green-digital.png',
        'Disney+': '/images/disney_logo_march_2024_050fef2e.png',
        'Prime Video': '/images/AmazonPrimeVideo.png',
        'Max': '/images/maxlogo.jpg',
        'Apple TV': '/images/AppleTV-iOS.png',
        'Peacock': '/images/Peacock_P.png',
        'Paramount+': '/images/Paramountplus.png',
        'Starz': '/images/Starz_Prism_Button_Option_01.png',
        'Tubi': '/images/tubitvlogo.png',
        'Pluto TV': '/images/Pluto-TV-Logo.jpg',
        'BritBox': '/images/britboxlogo.png',
        'AMC+': '/images/amcpluslogo.png'
    };
    
    return serviceLogos[serviceName] || ''; // Return empty string if no logo found
}

function getStreamingServiceLink(serviceName) {
    const serviceLinks = {
        'Netflix': 'https://www.netflix.com/login',
        'Hulu': 'https://auth.hulu.com/web/login',
        'Disney+': 'https://www.disneyplus.com/login',
        'Prime Video': 'https://www.primevideo.com',
        'Max': 'https://play.max.com/sign-in',
        'Apple TV': 'https://tv.apple.com/login',
        'Peacock': 'https://www.peacocktv.com/signin',
        'Paramount+': 'https://www.paramountplus.com/account/signin/',
        'Starz': 'https://www.starz.com/login',
        'Tubi': 'https://tubitv.com/login',
        'Pluto TV': 'https://pluto.tv/en/login',
        'BritBox': 'https://www.britbox.com/us/',
        'AMC+': 'https://www.amcplus.com/login'
    };
    
    return serviceLinks[serviceName] || '#'; // Return empty string if no logo found
};

// Update the recommendationsContainer event listener similarly
document.addEventListener('DOMContentLoaded', function() {
    let modal = null; // Store modal instance outside the click handler
    let isModalLoading = false; // Track if modal is currently loading

    document.getElementById('recommendationsContainer')?.addEventListener('click', async function(event) {
        if (event.target.classList.contains('btn-primary') && 
            event.target.textContent.trim() === 'View Details') {
            
            // Prevent multiple clicks while loading
            if (isModalLoading) return;
            isModalLoading = true;
            event.target.disabled = true;
            
            const movieCard = event.target.closest('.movie-card');
            
            if (movieCard) {
                const titleElement = movieCard.querySelector('h5');
                const title = titleElement ? titleElement.textContent.replace(/\s*\(\d+\)$/, '').trim() : '';
                
                if (!title) {
                    console.error("Could not determine movie title");
                    isModalLoading = false;
                    event.target.disabled = false;
                    return;
                }

                try {
                    // Initialize modal if not already done
                    if (!modal) {
                        modal = new bootstrap.Modal(document.getElementById('movieModal'));
                    }
                    
                    // Hide any existing modal immediately
                    modal.hide();
                    
                    // Clear previous content and show loading state
                    document.getElementById('modalTitle').textContent = "Loading...";
                    document.getElementById('modalPoster').src = '';
                    document.getElementById('modalGenres').textContent = '';
                    document.getElementById('modalRating').textContent = '';
                    document.getElementById('modalOverview').textContent = '';
                    
                    const streamingContainer = document.getElementById('modalStreaming');
                    streamingContainer.innerHTML = "<div class='text-center'>Loading...</div>";
                    
                    // Show modal immediately with loading state
                    modal.show();
                    const response = await fetch(`/Home/SearchMovies?query=${encodeURIComponent(title)}`);
                    const results = await response.json();
                    
                    if (!results || results.length === 0) {
                        throw new Error("No results found");
                    }
                    
                    const movie = results[0];
                    
                    // Update modal content
                    document.getElementById('modalTitle').textContent = `${movie.title} (${movie.releaseYear || 'N/A'})`;
                    document.getElementById('modalGenres').textContent = 
                        `Genres: ${movie.genres?.join(", ") || "Unknown"}`;
                    document.getElementById('modalRating').textContent = 
                        `Rating: ${movie.rating || "No rating available"}`;
                    document.getElementById('modalOverview').textContent = 
                        `Overview: ${movie.overview || "No overview available"}`;
                    
                    // Clear and add streaming services
                    streamingContainer.innerHTML = '';
                    const addedServices = new Set();
                    
                    if (movie.services && movie.services.length > 0) {
                        movie.services.forEach(service => {
                            const trimmedService = service.trim();
                            if (trimmedService && !addedServices.has(trimmedService)) {
                                addedServices.add(trimmedService);
                                
                                const link = document.createElement("a");
                                link.href = getStreamingServiceLink(trimmedService);
                                link.target = "_blank";
                                link.rel = "noopener noreferrer";
                                
                                const icon = document.createElement("img");
                                icon.src = getStreamingServiceLogo(trimmedService);
                                icon.alt = trimmedService;
                                icon.title = trimmedService;
                                icon.className = "streaming-icon";
                                icon.style.width = "40px";
                                icon.style.height = "40px";
                                icon.style.objectFit = "contain";
                                icon.style.marginRight = "5px";
                                
                                link.appendChild(icon);
                                streamingContainer.appendChild(link);
                            }
                        });
                    } else {
                        streamingContainer.textContent = "Not available on streaming platforms.";
                    }
                    
                    // Set poster image
                    const posterImg = document.getElementById('modalPoster');
                    posterImg.src = movie.posterUrl || 'https://via.placeholder.com/150';
                    posterImg.alt = `${movie.title} poster`;
                    
                } catch (error) {
                    console.error("Error fetching movie details:", error);
                    
                    document.getElementById('modalTitle').textContent = "Error";
                    document.getElementById('modalGenres').textContent = "";
                    document.getElementById('modalOverview').textContent = 
                        "Could not load movie details. Please try again.";
                    document.getElementById('modalStreaming').textContent = "";
                    
                    if (modal) {
                        modal.show();
                    }
                } finally {
                    isModalLoading = false;
                    event.target.disabled = false;
                }
            }
        }
    });
    
    // Handle modal hidden event to clean up
    document.getElementById('movieModal')?.addEventListener('hidden.bs.modal', function() {
        // Reset modal state when hidden
        isModalLoading = false;
    });
});