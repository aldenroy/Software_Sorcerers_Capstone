async function searchMovies() {
    let searchInput = document.getElementById("searchInput");
    let query = searchInput.value.trim();
    let resultsContainer = document.getElementById("results");

    // Clear previous results
    resultsContainer.innerHTML = "";

    // Check if the search box is empty
    if (query === "") {
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>Please enter a movie title before searching.</div>";
        searchInput.focus(); // Focus back on input for better UX
        return;
    }

    let response = await fetch(`/Home/SearchMovies?query=${encodeURIComponent(query)}`);
    let movies = await response.json();

    if (movies.length === 0) {
        resultsContainer.innerHTML = "<div class='no-results' role='alert'>No results found.</div>";
        return;
    }

    movies.forEach(movie => {
        let movieCard = document.createElement("article");
        movieCard.className = "movie-card";
        movieCard.innerHTML = `
            <div class="movie-row" aria-label="Search results card for ${movie.title}">
                <img src="${movie.posterUrl || 'https://via.placeholder.com/150'}" class="movie-poster" alt="${movie.title} movie poster">
                <div class="movie-details" aria-label="Movie details for ${movie.title}">
                    <h5 aria-label="Movie title is ${movie.title}">${movie.title} <span class="movie-year">(${movie.releaseYear || 'N/A'})</span></h5>
                    <p class="movie-genres">Genres: ${movie.genres?.join(", ") || 'Unknown'}</p>
                    <button class="btn btn-primary" aria-label="View details for ${movie.title}">View Details</button>
                    <button class="btn btn-outline-secondary" aria-label="Find more movies like ${movie.title}">More Like This</button>
                </div>
            </div>
        `;
        resultsContainer.appendChild(movieCard);
    });
}

function handleKeyPress(event) {
    if (event.key === "Enter") {
        searchMovies();
    }
}
