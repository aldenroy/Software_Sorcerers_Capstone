// document.addEventListener("DOMContentLoaded", () => {
//     //listens for the dynamically added elements
//     document.body.addEventListener("click", (event) => {
//         if (event.target.matches(".btn-primary")) {
//             openModal(event.target);
//         }
//     });
// });

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("results").addEventListener("click", function (event) {
        if (event.target.classList.contains("btn-primary")) {
            let movieCard = event.target.closest(".movie-card");

            let title = movieCard.querySelector("h5").textContent;
            let posterUrl = movieCard.querySelector("img").src;
            let genres = movieCard.querySelector(".movie-genres").textContent.replace("Genres: ", "");
            let rating = movieCard.querySelector(".movie-rating").textContent.replace("Rating: ", "");
            
            let overview = movieCard.getAttribute("data-overview") || "No overview available.";
            let streamingServices = movieCard.getAttribute("data-streaming") || "Not available on streaming platforms.";

            //populate the modal with movie details
            document.getElementById("modalTitle").textContent = title;
            document.getElementById("modalPoster").src = posterUrl;
            document.getElementById("modalGenres").textContent = `Genres: ${genres}`;
            document.getElementById("modalRating").textContent = `Rating: ${rating}`;
            document.getElementById("modalOverview").textContent = `Overview: ${overview}`;
            document.getElementById("modalStreaming").textContent = `Streaming Services: ${streamingServices}`;

            // Show the modal
            var movieModal = new bootstrap.Modal(document.getElementById("movieModal"));
            movieModal.show();
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("similar-movie").addEventListener("click", function (event) {
        if (event.target.classList.contains("btn-primary")) {
            console.log("test")
        }
    });
});

document.addEventListener('DOMContentLoaded', function() {
    // Set up event delegation for the view details buttons
    document.getElementById('recommendationsContainer')?.addEventListener('click', async function(event) {
        // Check if a view details button was clicked
        if (event.target.classList.contains('btn-primary') && 
            event.target.textContent.trim() === 'View Details') {
            
            console.log("View Details button clicked - fetching movie data");
            
            const movieCard = event.target.closest('.movie-card');
            
            if (movieCard) {
                const titleElement = movieCard.querySelector('h5');
                const title = titleElement ? titleElement.textContent.replace(/\s*\(\d+\)$/, '').trim() : '';
                
                if (!title) {
                    console.error("Could not determine movie title");
                    return;
                }

                try {
                    const modalTitle = document.getElementById('modalTitle');
                    modalTitle.textContent = "Loading...";
                    
                    // Fetch movie details from API
                    const response = await fetch(`/Home/SearchMovies?query=${encodeURIComponent(title)}`);
                    const results = await response.json();
                    
                    if (!results || results.length === 0) {
                        throw new Error("No results found");
                    }
                    
                    const movie = results[0];
                    
                    modalTitle.textContent = `${movie.title} (${movie.releaseYear || 'N/A'})`;
                    document.getElementById('modalGenres').textContent = 
                        `Genres: ${movie.genres?.join(", ") || "Unknown"}`;

                    document.getElementById('modalRating').textContent = 
                        `Rating: ${movie.rating || "No rating available"}`;

                    document.getElementById('modalOverview').textContent = 
                        `Overview: ${movie.overview || "No overview available"}`;

                    document.getElementById('modalStreaming').textContent = 
                        `Available on: ${movie.services?.join(", ") || "Not available on any streaming services"}`;
                    
                    const posterImg = document.getElementById('modalPoster');
                    posterImg.src = movie.posterUrl || 'https://via.placeholder.com/150';
                    posterImg.alt = `${movie.title} poster`;
                    
                    const modal = new bootstrap.Modal(document.getElementById('movieModal'));
                    modal.show();
                    
                } catch (error) {
                    console.error("Error fetching movie details:", error);
                    
                    document.getElementById('modalTitle').textContent = "Error";
                    document.getElementById('modalGenres').textContent = "";
                    document.getElementById('modalOverview').textContent = 
                        "Could not load movie details. Please try again.";
                    document.getElementById('modalStreaming').textContent = "";
                    
                    const modal = new bootstrap.Modal(document.getElementById('movieModal'));
                    modal.show();
                }
            }
        }
    });
});