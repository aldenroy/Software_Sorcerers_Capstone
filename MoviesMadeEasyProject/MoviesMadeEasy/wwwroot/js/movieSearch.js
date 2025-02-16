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
    
    // Get sort option from dropdown
    let sortOption = document.getElementById("sortBy").value;

    // Get year filter values (if any)
    let minYearValue = document.getElementById("minYear").value.trim();
    let maxYearValue = document.getElementById("maxYear").value.trim();

    let minYear = minYearValue ? parseInt(minYearValue, 10) : null;
    let maxYear = maxYearValue ? parseInt(maxYearValue, 10) : null;

    // If one value is provided, use it to filter for that specific year.
    if (minYear !== null && maxYear === null) {
        maxYear = minYear;
    } else if (maxYear !== null && minYear === null) {
        minYear = maxYear;
    }

    // Filter movies by year range if either min or max is provided
    let filteredMovies = movies.filter(movie => {
        let year = movie.releaseYear || 0;
        let valid = true;
        if (minYear !== null) {
            valid = valid && (year >= minYear);
        }
        if (maxYear !== null) {
            valid = valid && (year <= maxYear);
        }
        return valid;
    });

    if (filteredMovies.length === 0) {
        resultsContainer.innerHTML = "<div class='no-results' role='alert'>No movies match the selection.</div>";
        return;
    }

    // Sort the filtered movies
    if (sortOption === "yearAsc") {
        filteredMovies.sort((a, b) => (a.releaseYear || 0) - (b.releaseYear || 0));
    } else if (sortOption === "yearDesc") {
        filteredMovies.sort((a, b) => (b.releaseYear || 0) - (a.releaseYear || 0));
    }

    // Render movie cards from the filtered & sorted list
    filteredMovies.forEach(movie => {
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