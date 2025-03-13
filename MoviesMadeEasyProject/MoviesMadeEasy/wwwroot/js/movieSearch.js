
let searchExecuted = false;
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
        let response = await fetch(`/Home/SearchMovies?${queryParams.toString()}`);
        let index = await response.json();

        loadingSpinner.style.display = "none";

        if (!index || index.length === 0) {
            resultsContainer.innerHTML = "<div class='no-results' role='alert'>No results found.</div>";
            updateClearFiltersVisibility();
            return;
        }

        resultsContainer.innerHTML = index.map(item => {
            let genres = item.genres && item.genres.length > 0 
                ? item.genres.join(", ") 
                : 'Unknown';
        
            // Ensure overview and services are correctly mapped
            let overview = item.overview || 'N/A';
            let services = item.services && item.services.length > 0 
                ? item.services.join(', ') 
                : 'N/A';
            console.log(overview)
            console.log(services)
        
            return `
                <article class="movie-card" 
                        data-overview="${overview}" 
                        data-streaming="${services}">
                    <div class="movie-row" aria-label="Search results card for ${item.title}">
                        <img src="${item.posterUrl || 'https://via.placeholder.com/150'}" class="movie-poster" alt="${item.title} movie poster">
                        <div class="movie-details">
                            <h5>${item.title} <span class="movie-year">(${item.releaseYear || 'N/A'})</span></h5>
                            <p class="movie-genres">Genres: ${genres}</p>
                            <p class="movie-rating">Rating: ${item.rating || 'N/A'}</p>
                            <button class="btn btn-primary">View Details</button>
                            <button class="btn btn-outline-secondary">More Like This</button>
                        </div>
                    </div>
                </article>
            `;
        }).join('');

        // Enable filters once a successful search is executed.
        enableFilters();

        // Show Clear Filters if any filters are applied.
        updateClearFiltersVisibility();
    } catch (error) {
        loadingSpinner.style.display = "none";
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>An error occurred while fetching data. Please try again later.</div>";
        console.error("Error fetching index:", error);
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>An error occurred while fetching data. Please try again later.</div>";
        console.error("Error fetching index:", error);
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

function updateClearFiltersVisibility() {
    let sortOption = document.getElementById("sortBy").value;
    let minYear = document.getElementById("minYear").value.trim();
    let maxYear = document.getElementById("maxYear").value.trim();
    let clearButton = document.getElementById("clearFilters");

    // Show button if any filter is not its default/empty
    if (sortOption !== "default" || minYear !== "" || maxYear !== "") {
        clearButton.style.display = "inline-block";
    } else {
        clearButton.style.display = "none";
    }
}

function clearFilters() {
    // Reset the filter values
    document.getElementById("sortBy").value = "default";
    document.getElementById("minYear").value = "";
    document.getElementById("maxYear").value = "";
    document.getElementById("clearFilters").style.display = "none";

    // Optionally, re-trigger the search to display the full list
    searchMovies();
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

     // Attach event listener for the Clear Filters button
     document.getElementById("clearFilters").addEventListener("click", clearFilters);
});

module.exports = { searchMovies };