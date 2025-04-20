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
    const genreFiltersContainer = document.getElementById("genre-filters");

    // Clear previous results
    resultsContainer.innerHTML = "";
    loadingSpinner.style.display = "block";

    if (query === "") {
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>Please enter a movie title before searching.</div>";
        searchInput.focus();
        loadingSpinner.style.display = "none";
        return;
    }

    // Convert year values to numbers
    let numMinYear = minYear ? parseInt(minYear, 10) : NaN;
    let numMaxYear = maxYear ? parseInt(maxYear, 10) : NaN;

    // Validate date range if both values are provided
    if (minYear && maxYear) {
        if (isNaN(numMinYear) || isNaN(numMaxYear) || numMinYear > numMaxYear) {
            resultsContainer.innerHTML = "<div class='error-message' role='alert'>Please enter a valid date range: Min Year must be less than or equal to Max Year.</div>";
            loadingSpinner.style.display = "none";
            return;
        }
    }

    // Construct query parameters
    let queryParams = new URLSearchParams({
        query: query,
        sortBy: sortOption
    });
    if (minYear) queryParams.append("minYear", minYear);
    if (maxYear) queryParams.append("maxYear", maxYear);

    try {
        console.log("Fetching: /Home/SearchMovies?" + queryParams.toString());
        let response = await fetch(`/Home/SearchMovies?${queryParams.toString()}`);
        let index = await response.json();

        loadingSpinner.style.display = "none";

        if (!index || index.length === 0) {
            resultsContainer.innerHTML = "<div class='no-results' role='alert'>No results found.</div>";
            updateClearFiltersVisibility();
            return;
        }

        const availableGenresSet = new Set();
        resultsContainer.innerHTML = index.map(item => {
            // Add any genres that exist in the movie
            if(item.genres && item.genres.length) {
                item.genres.forEach(genre => availableGenresSet.add(genre));
            }
            let overview = item.overview || 'N/A';
            let services = item.services && item.services.length > 0 
                ? item.services.join(', ') 
                : 'N/A';
            console.log(overview)
            console.log(services)

            // Prepare genre CSV (for the data-genres attribute)
            const genresCSV = item.genres && item.genres.length ? item.genres.join(",") : "";
            return `
                <article class="movie-card" data-genres="${genresCSV}" data-overview="${overview}" data-streaming="${services}">
                    <div class="movie-row" aria-label="Search results card for ${item.title}">
                        <img src="${item.posterUrl || 'https://via.placeholder.com/150'}" class="movie-poster" alt="${item.title} movie poster">
                        <div class="movie-details">
                            <h5>${item.title} <span class="movie-year">(${item.releaseYear || 'N/A'})</span></h5>
                            <p class="movie-genres">Genres: ${item.genres && item.genres.length ? item.genres.join(", ") : 'Unknown'}</p>
                            <p class="movie-rating">Rating: ${item.rating || 'N/A'}</p>
                            <button class="btn btn-primary btn-view-details">View Details</button>
                            <button class="btn btn-outline-secondary">More Like This</button>
                        </div>
                    </div>
                </article>
            `;
        }).join('');

        // Inside your searchMovies function (after processing search results)
        if (availableGenresSet.size > 0) {
            const genreFiltersContainer = document.getElementById("genre-filters");
            genreFiltersContainer.style.display = "block";
            const availableGenres = Array.from(availableGenresSet);
            setupGenreFilter(availableGenres);
        } else {
            document.getElementById("genre-filters").style.display = "none";
        }
        
        // Ensure filters are enabled now that a search was executed
        enableFilters();
        updateStreamingFilters();
        updateClearFiltersVisibility();
    } catch (error) {
        loadingSpinner.style.display = "none";
        resultsContainer.innerHTML = "<div class='error-message' role='alert'>An error occurred while fetching data. Please try again later.</div>";
        console.error("Error fetching index:", error);
    }
}

function setupGenreFilter(availableGenres) {
    const filterContainer = document.getElementById("genre-filters");
    const contentList = document.getElementById("results");

    // Load saved filter state (if any)
    const savedFilters = localStorage.getItem("selectedGenres");
    let selectedGenres = savedFilters ? JSON.parse(savedFilters) : [];

    // Render genre checkboxes
    filterContainer.innerHTML = "";
    availableGenres.forEach(genre => {
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.value = genre;
        checkbox.id = `genre-${genre}`;
        if (selectedGenres.indexOf(genre) > -1) {
            checkbox.checked = true;
        }

        const label = document.createElement("label");
        label.setAttribute("for", checkbox.id);
        label.textContent = genre;

        const wrapper = document.createElement("div");
        wrapper.appendChild(checkbox);
        wrapper.appendChild(label);
        filterContainer.appendChild(wrapper);
    });

    function filterContent() {
        // Get currently selected genres
        selectedGenres = Array.from(filterContainer.querySelectorAll("input[type='checkbox']:checked"))
            .map(cb => cb.value);
        localStorage.setItem("selectedGenres", JSON.stringify(selectedGenres));

        // Filter movie cards: only show movies that include ALL selected genres.
        Array.from(contentList.children).forEach(item => {
            const genresAttr = item.getAttribute("data-genres") || "";
            const itemGenres = genresAttr.split(",").map(s => s.trim()).filter(s => s);
            if (
                selectedGenres.length === 0 ||
                selectedGenres.every(genre => itemGenres.includes(genre))
            ) {
                item.style.display = "";
            } else {
                item.style.display = "none";
            }
        });

        updateClearFiltersVisibility();
    }

    // Listen for checkbox changes
    filterContainer.addEventListener("change", filterContent);

    // Execute filter to restore previous state on load
    filterContent();
}

document.addEventListener("DOMContentLoaded", () => {
    // Use the full list of genres from your API
    const allGenres = [
        "Action", "Adult", "Adventure", "Animation", "Biography", "Comedy", "Crime",
        "Documentary", "Drama", "Family", "Fantasy", "Film Noir", "Game Show", "History",
        "Horror", "Musical", "Music", "Mystery", "News", "Reality-TV", "Romance",
        "Sci-Fi", "Short", "Sport", "Talk-Show", "Thriller", "War", "Western"
    ];
    const availableStreamingServices = ["Netflix", "Hulu", "Disney+", "Amazon Prime Video", "Max \"HBO Max\"", "Apple TV+", "Peacock", "Starz", "Tubi", "Pluto TV", "BritBox", "AMC+"];
    setupStreamingFilter(availableStreamingServices);
    setupGenreFilter(allGenres);
});

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
    // Use the value from "sortBy" if it exists; otherwise, default to "default"
    let sortOption = document.getElementById("sortBy")?.value || "default";
    let minYear = document.getElementById("minYear")?.value?.trim() || "";
    let maxYear = document.getElementById("maxYear")?.value?.trim() || "";
    let clearButton = document.getElementById("clearFilters");

    const genreCheckboxes = document.querySelectorAll("#genre-filters input[type='checkbox']");
    const isGenreFilterApplied = Array.from(genreCheckboxes).some(cb => cb.checked);

    const streamingCheckboxes = document.querySelectorAll("#streaming-filters input[type='checkbox']");
    const isStreamingFilterApplied = Array.from(streamingCheckboxes).some(cb => cb.checked);

    // Show the Clear Filters button if any filter is applied
    if (sortOption !== "default" || minYear !== "" || maxYear !== "" || isGenreFilterApplied || isStreamingFilterApplied) {
        clearButton.style.display = "inline-block";
    } else {
        clearButton.style.display = "none";
    }
}

function filterContentByStreaming() {
    const selectedServices = Array.from(document.querySelectorAll("#streaming-filters input[type='checkbox']:checked"))
        .map(cb => cb.value);
    const movieCards = document.querySelectorAll(".movie-card");
    movieCards.forEach(card => {
        const serviceAttr = card.getAttribute("data-streaming") || "";
        const cardServices = serviceAttr.split(",").map(s => s.trim()).filter(s => s);
        if (selectedServices.length === 0 || selectedServices.some(service => cardServices.includes(service))) {
            card.style.display = "block";
        } else {
            card.style.display = "none";
        }
    });
    updateClearFiltersVisibility();
}

function setupStreamingFilter(availableServices) {
    const streamingFilterContainer = document.getElementById("streaming-filters");
    if (!streamingFilterContainer) return;
    streamingFilterContainer.innerHTML = "";
    availableServices.forEach(service => {
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.value = service;
        checkbox.id = `streaming-${service}`;
    
        const label = document.createElement("label");
        label.setAttribute("for", checkbox.id);
        label.textContent = service;
    
        const wrapper = document.createElement("div");
        wrapper.appendChild(checkbox);
        wrapper.appendChild(label);
    
        streamingFilterContainer.appendChild(wrapper);
    });
    // Listen for changes on streaming checkboxes
    streamingFilterContainer.addEventListener("change", filterContentByStreaming);
}

function updateStreamingFilters() {
    // Get all movie cards that are part of the search results.
    const movieCards = document.querySelectorAll(".movie-card");
    let streamingServicesSet = new Set();

    movieCards.forEach(card => {
        const servicesText = card.getAttribute("data-streaming") || "";
        servicesText.split(",")
            .map(s => s.trim())
            .filter(s => s)
            .forEach(service => streamingServicesSet.add(service));
    });
    
    const streamingServices = Array.from(streamingServicesSet).sort();

    // Preserve already selected streaming services
    const selectedServices = new Set(Array.from(document.querySelectorAll("#streaming-filters input[type='checkbox']:checked"))
        .map(cb => cb.value));

    const container = document.getElementById("streaming-filters");
    container.innerHTML = "";
    
    streamingServices.forEach(service => {
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.value = service;
        checkbox.id = `streaming-${service}`;

        // Restore selection if the service is already selected.
        if (selectedServices.has(service)) {
            checkbox.checked = true;
        }

        const label = document.createElement("label");
        label.setAttribute("for", checkbox.id);
        label.textContent = service;
    
        const wrapper = document.createElement("div");
        wrapper.appendChild(checkbox);
        wrapper.appendChild(label);
    
        container.appendChild(wrapper);
    });
    
    container.addEventListener("change", filterContentByStreaming);
}

function clearFilters() {
    document.getElementById("sortBy").value = "default";
    document.getElementById("minYear").value = "";
    document.getElementById("maxYear").value = "";

    const genreCheckboxes = document.querySelectorAll("#genre-filters input[type='checkbox']");
    genreCheckboxes.forEach(cb => {
        cb.checked = false;
    });

    const streamingCheckboxes = document.querySelectorAll("#streaming-filters input[type='checkbox']");
    streamingCheckboxes.forEach(cb => {
        cb.checked = false;
    });

    document.getElementById("clearFilters").style.display = "none";
    localStorage.removeItem("selectedGenres");
    
    // Re-trigger the search to rebuild the unfiltered content list.
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
    localStorage.removeItem("selectedGenres");

    const allGenres = [
        "Action", "Adult", "Adventure", "Animation", "Biography", "Comedy", "Crime",
        "Documentary", "Drama", "Family", "Fantasy", "Film Noir", "Game Show", "History",
        "Horror", "Musical", "Music", "Mystery", "News", "Reality-TV", "Romance",
        "Sci-Fi", "Short", "Sport", "Talk-Show", "Thriller", "War", "Western"
    ];
    setupGenreFilter(allGenres);

    // When attaching event listeners for "sortBy", check if the element exists
    let sortByElem = document.getElementById("sortBy");
    if (sortByElem) {
        sortByElem.addEventListener("change", updateClearFiltersVisibility);
        sortByElem.addEventListener("click", handleFilterInteraction);
    }

    document.getElementById("minYear").addEventListener("input", updateClearFiltersVisibility);
    document.getElementById("maxYear").addEventListener("input", updateClearFiltersVisibility);

    document.getElementById("minYear").addEventListener("focus", handleFilterInteraction);
    document.getElementById("maxYear").addEventListener("focus", handleFilterInteraction);
    
     // Attach event listener for the Clear Filters button
     document.getElementById("clearFilters").addEventListener("click", clearFilters);
});

document.querySelectorAll(".sort-option").forEach(item => {
    item.addEventListener("click", (e) => {
        e.preventDefault();
        let sortBy = e.target.getAttribute("data-sort");
        document.getElementById("sortGenreDropdown").textContent = e.target.textContent;
        // Update the hidden sortBy input
        document.getElementById("sortBy").value = sortBy;
        searchMovies();
    });
});

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { searchMovies };
}