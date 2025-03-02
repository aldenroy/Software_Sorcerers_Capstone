async function searchMovies() {
    let searchInput = document.getElementById("searchInput");
    let query = searchInput.value.trim();
    let resultsContainer = document.getElementById("results");
    let loadingSpinner = document.getElementById("loadingSpinner");

    // Get filter and sorting values
    let sortOption = document.getElementById("sortBy")?.value || "default";
    let minYear = document.getElementById("minYear")?.value?.trim();
    let maxYear = document.getElementById("maxYear")?.value?.trim();

    // Clear previous results
    resultsContainer.innerHTML = "";
    loadingSpinner.style.display = "block";

    if (query === "") {
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>Please enter a movie title before searching.</div>";
        searchInput.focus();
        loadingSpinner.style.display = "none";
        return;
    }

    // Construct query parameters
    //did this so I can add new stuff to help do the logic in the controller.
    let queryParams = new URLSearchParams({
        query: query,
        sortBy: sortOption
    });

    // Add minYear and maxYear if they are provided
    // Again another think to help in the controller logic. Gets from dropdown
    if (minYear) queryParams.append("minYear", minYear);
    if (maxYear) queryParams.append("maxYear", maxYear);

    try {
        let response = await fetch(`/Home/SearchMovies?${queryParams.toString()}`);
        let movies = await response.json();

        loadingSpinner.style.display = "none";

        if (!movies || movies.length === 0) {
            resultsContainer.innerHTML = "<div class='no-results' role='alert'>No results found.</div>";
            return;
        }

        resultsContainer.innerHTML = movies.map(movie => `
            <article class="movie-card">
                <div class="movie-row" aria-label="Search results card for ${movie.title}">
                    <img src="${movie.posterUrl || 'https://via.placeholder.com/150'}" class="movie-poster" alt="${movie.title} movie poster">
                    <div class="movie-details">
                        <h5>${movie.title} <span class="movie-year">(${movie.releaseYear || 'N/A'})</span></h5>
                        <p class="movie-genres">Genres: ${movie.genres?.join(", ") || 'Unknown'}</p>
                        <p class="movie-rating">Rating: ${movie.rating || 'N/A'}</p>
                        <button class="btn btn-primary">View Details</button>
                        <button class="btn btn-outline-secondary">More Like This</button>
                    </div>
                </div>
            </article>
        `).join('');

        enableFilters();
    } catch (error) {
        loadingSpinner.style.display = "none";
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>An error occurred while fetching movie data. Please try again later.</div>";
        console.error("Error fetching movies:", error);
    }
}

function enableFilters() {
    searchExecuted = true;
    document.getElementById("sortBy").disabled = false;
    document.getElementById("minYear").disabled = false;
    document.getElementById("maxYear").disabled = false;
}

function handleFilterInteraction(event) {
    if (!searchExecuted) {
        event.preventDefault();
        alert("Please perform a search to use filters");
    }
}
document.addEventListener("DOMContentLoaded", () => {
    let searchInput = document.getElementById("searchInput");

    // Trigger search when Enter is pressed in the input field
    searchInput.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault(); // Prevent form submission (if inside a form)
            searchMovies();
        }
    });

    document.getElementById("sortBy").addEventListener("click", handleFilterInteraction);
    document.getElementById("minYear").addEventListener("focus", handleFilterInteraction);
    document.getElementById("maxYear").addEventListener("focus", handleFilterInteraction);
});

module.exports = { searchMovies };