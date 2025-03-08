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
    let queryParams = new URLSearchParams({
        query: query,
        sortBy: sortOption
    });

    // Add minYear and maxYear if they are provided
    if (minYear) queryParams.append("minYear", minYear);
    if (maxYear) queryParams.append("maxYear", maxYear);

    try {
        let response = await fetch(`/Home/SearchIndex?${queryParams.toString()}`);
        let index = await response.json();

        loadingSpinner.style.display = "none";

        if (!index || index.length === 0) {
            resultsContainer.innerHTML = "<div class='no-results' role='alert'>No results found.</div>";
            return;
        }

        resultsContainer.innerHTML = index.map(item => {
            // Assuming each genre is an object with a 'name' property
            let genres = item.genres && item.genres.length > 0 
                         ? item.genres.map(genre => genre.name).join(", ") 
                         : 'Unknown';

            return `
                <article class="movie-card">
                    <div class="movie-row" aria-label="Search results card for ${item.title}">
                        <img src="${item.posterUrl || 'https://via.placeholder.com/150'}" class="movie-poster" alt="${item.title} movie poster">
                        <div class="movie-details">
                            <h5>${item.title} <span class="movie-year">(${item.releaseYear || 'N/A'})</span></h5>
                            
                            <!-- Display genres here -->
                            <p class="movie-genres">Genres: ${genres}</p>
                            
                            <p class="movie-rating">Rating: ${item.rating || 'N/A'}</p>
                            <button class="btn btn-primary">View Details</button>
                            <button class="btn btn-outline-secondary">More Like This</button>
                        </div>
                    </div>
                </article>
            `;
        }).join('');

        enableFilters();
    } catch (error) {
        loadingSpinner.style.display = "none";
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>An error occurred while fetching data. Please try again later.</div>";
        console.error("Error fetching index:", error);
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