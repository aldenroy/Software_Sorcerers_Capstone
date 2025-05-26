const allGenres = [
  "Action","Adult","Adventure","Animation","Biography","Comedy","Crime",
  "Documentary","Drama","Family","Fantasy","Film Noir","Game Show","History",
  "Horror","Musical","Music","Mystery","News","Reality-TV","Romance",
  "Sci-Fi","Short","Sport","Talk-Show","Thriller","War","Western"
];

const availableStreamingServices = [
  "Netflix","Hulu","Disney+","Amazon Prime Video","Max \"HBO Max\"",
  "Apple TV","Peacock","Starz","Tubi","Pluto TV","BritBox","AMC+"
];

// restore any saved genres, start with no services selected
let selectedGenres = new Set(
  Array.isArray(JSON.parse(localStorage.getItem("selectedGenres"))) ? 
  JSON.parse(localStorage.getItem("selectedGenres")) : []
);

let selectedServices = [];
let searchExecuted = true;
const NO_ITEMS_HTML = "<div class='no-results' role='alert'>>No results found.</div>";
const NO_MOVIES_RANGE_HTML = "<div class='no-results' role='alert'>No movies found for that rating range.</div>";
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
  let minRating = document.getElementById("minRatingTextBox")?.value?.trim();
  let maxRating = document.getElementById("maxRatingTextBox")?.value?.trim();

  selectedGenres = new Set(
    Array.from(genreFiltersContainer.querySelectorAll("input[type='checkbox']:checked"))
      .map(cb => cb.value)
  );

  localStorage.setItem("selectedGenres", JSON.stringify(Array.from(selectedGenres)));

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

  let numMinRating = minRating ? parseFloat(minRating) : NaN;
  let numMaxRating = maxRating ? parseFloat(maxRating) : NaN;

  // Validate rating range if both provided
  if (minRating && maxRating) {
    if (isNaN(numMinRating) || isNaN(numMaxRating) || numMinRating > numMaxRating) {
      resultsContainer.innerHTML =
        "<div class='error-message' role='alert'>" +
        "The rating range is invalid: Min Rating must be ≤ Max Rating." +
        "</div>";
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
      resultsContainer.innerHTML = NO_ITEMS_HTML;
      updateClearFiltersVisibility();
      return;
    }

    if (minRating && maxRating) {
      const filtered = index.filter(item => {
        const r = parseFloat(item.rating);
        return !isNaN(r) && r >= numMinRating && r <= numMaxRating;
      });
      if (filtered.length === 0) {
        resultsContainer.innerHTML = NO_MOVIES_RANGE_HTML;
        updateClearFiltersVisibility();
        return;
      }
      // use the pruned list for rendering
      index = filtered;
    }

    // Apply sorting logic based on the selected option
    if (sortOption === "yearAsc") {
      index.sort((a, b) => a.releaseYear - b.releaseYear);
    } else if (sortOption === "yearDesc") {
      index.sort((a, b) => b.releaseYear - a.releaseYear);
    } else if (sortOption === "titleAsc") {
      index.sort((a, b) => a.title.localeCompare(b.title));
    } else if (sortOption === "titleDesc") {
      index.sort((a, b) => b.title.localeCompare(a.title));
    } else if (sortOption === "ratingHighLow") {
      index.sort((a, b) => b.rating - a.rating);
    } else if (sortOption === "ratingLowHigh") {
      index.sort((a, b) => a.rating - b.rating);
    }

    const availableGenresSet = new Set();
    resultsContainer.innerHTML = index.map(item => {
      // Add any genres that exist in the movie
      if (item.genres && item.genres.length) {
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
      setupGenreFilter(allGenres, availableGenres);
    } else {
      document.getElementById("genre-filters").style.display = "none";
    }

    // Ensure filters are enabled now that a search was executed
    enableFilters();
    updateStreamingFilters();
    updateClearFiltersVisibility();
    setupLetterFilterDropdown();
  } catch (error) {
    loadingSpinner.style.display = "none";
    resultsContainer.innerHTML = "<div class='error-message' role='alert'>An error occurred while fetching data. Please try again later.</div>";
    console.error("Error fetching index:", error);
  }
}

function updateMinYearLabel() {
  const slider = document.getElementById("minYear");
  const box = document.getElementById("minYearTextBox");
  if (slider && box) box.value = slider.value;
}

function updateMaxYearLabel() {
  const slider = document.getElementById("maxYear");
  const box = document.getElementById("maxYearTextBox");
  if (slider && box) box.value = slider.value;
}

function updateMinYearFromTextBox() {
  const slider = document.getElementById("minYear");
  const box = document.getElementById("minYearTextBox");
  if (!slider || !box) return;

  let val = parseInt(box.value, 10);
  const min = parseInt(slider.min, 10);
  const max = parseInt(slider.max, 10);

  if (isNaN(val)) {
    box.value = slider.value;
    return;
  }
  if (val < min) val = min;
  if (val > max) val = max;

  slider.value = val;
  box.value = val;

  if (!searchExecuted) {
    // if no search yet, show your “please search first” alert
    handleFilterInteraction({ target: slider, preventDefault: () => { } });
  } else {
    // otherwise re-run with the new year filter
    searchMovies();
  }
}

function updateMaxYearFromTextBox() {
  const slider = document.getElementById("maxYear");
  const box = document.getElementById("maxYearTextBox");
  if (!slider || !box) return;

  let val = parseInt(box.value, 10);
  const min = parseInt(slider.min, 10);
  const max = parseInt(slider.max, 10);

  if (isNaN(val)) {
    box.value = slider.value;
    return;
  }
  if (val < min) val = min;
  if (val > max) val = max;

  slider.value = val;
  box.value = val;

  if (!searchExecuted) {
    handleFilterInteraction({ target: slider, preventDefault: () => { } });
  } else {
    searchMovies();
  }
}

function setupGenreFilter(masterGenres, availableGenres) {
  const container = document.getElementById("genre-filters");
  container.innerHTML = "";  // Clear previous filters

  masterGenres.forEach(genre => {
    const wrapper  = document.createElement("div");
    const checkbox = document.createElement("input");
    checkbox.type  = "checkbox";
    checkbox.id    = `genre-${genre}`;
    checkbox.value = genre;

    // Disable and gray-out genres that aren't available in the search results
    if (!availableGenres.includes(genre)) {
      checkbox.disabled = true;
      wrapper.classList.add("text-muted", "opacity-50");
    }

    // Re-check if the user had it selected before
    if (selectedGenres.has(genre)) {
      checkbox.checked = true;
    }

    const label = document.createElement("label");
    label.htmlFor    = checkbox.id;
    label.textContent = genre;

    wrapper.append(checkbox, label);
    container.appendChild(wrapper);
  });

  // Bind the filter action to update genres selection
  container.addEventListener("change", filterContent);
}


function filterContent() {
  // which genres are checked right now?
  const filterContainer = document.getElementById("genre-filters");
  selectedGenres = Array.from(
    filterContainer.querySelectorAll("input[type='checkbox']:checked")
  ).map(cb => cb.value);
  localStorage.setItem("selectedGenres", JSON.stringify(Array.from(selectedGenres)));

  // show/hide movie cards based on genre match
  const resultsContainer = document.getElementById("results");
  Array.from(resultsContainer.children).forEach(card => {
    const genresAttr = card.getAttribute("data-genres") || "";
    const itemGenres = genresAttr.split(",").map(s => s.trim());
    const keep =
      selectedGenres.size === 0 ||
      Array.from(selectedGenres).every(g => itemGenres.includes(g));
    card.style.display = keep ? "" : "none";
  });

  updateClearFiltersVisibility();
}

document.addEventListener("DOMContentLoaded", () => {
  // Use the full list of genres from the API
  const allGenres = [
    "Action", "Adult", "Adventure", "Animation", "Biography", "Comedy", "Crime",
    "Documentary", "Drama", "Family", "Fantasy", "Film Noir", "Game Show", "History",
    "Horror", "Musical", "Music", "Mystery", "News", "Reality-TV", "Romance",
    "Sci-Fi", "Short", "Sport", "Talk-Show", "Thriller", "War", "Western"
  ];
  const availableStreamingServices = ["Netflix", "Hulu", "Disney+", "Amazon Prime Video", "Max \"HBO Max\"", "Apple TV+", "Peacock", "Starz", "Tubi", "Pluto TV", "BritBox", "AMC+"];
  clearFilters(true); // clear filters on page load
  setupStreamingFilter(availableStreamingServices);
  setupGenreFilter(allGenres, allGenres);
  updateMinYearLabel();
  updateMaxYearLabel();
  ["minYear", "maxYear"].forEach(id => {
    const slider = document.getElementById(id);
    if (!slider) return;

    // keep the text‐box in sync as you drag
    slider.addEventListener("input",
      id === "minYear" ? updateMinYearLabel : updateMaxYearLabel
    );

    // on “change” (mouse up), fire your filter/search as before
    slider.addEventListener("change", e => {
      if (!searchExecuted) {
        handleFilterInteraction(e);
      } else {
        searchMovies();
      }
    });
  });
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
  const clearBtn = document.getElementById("clearFilters");

  // 1) sort default?
  const sortDefault = document.getElementById("sortBy").value === "default";

  // 2) year default?
  const minYearElem = document.getElementById("minYear");
  const maxYearElem = document.getElementById("maxYear");
  const yearDefault =
    minYearElem && maxYearElem
      ? minYearElem.value === minYearElem.min && maxYearElem.value === maxYearElem.max
      : true;

  // 3) rating default? (guard missing elements)
  const minRatingElem = document.getElementById("minRating");
  const maxRatingElem = document.getElementById("maxRating");
  const ratingDefault =
    minRatingElem && maxRatingElem
      ? minRatingElem.value === minRatingElem.min && maxRatingElem.value === maxRatingElem.max
      : true;

  // 4) any genres or streaming checked?
  const genresChecked = !!document.querySelector("#genre-filters input:checked");
  const streamingChecked = !!document.querySelector("#streaming-filters input:checked");
  const letterFilterApplied = document.getElementById("letter-filter-dropdown").value !== "";

  if (!sortDefault || !yearDefault || !ratingDefault || genresChecked || streamingChecked || letterFilterApplied) {
    clearBtn.style.display = "inline-block";
  } else {
    clearBtn.style.display = "none";
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
  // build a Set of all services actually in the displayed cards
  const availableSet = new Set();
  document.querySelectorAll(".movie-card").forEach(card => {
    const svcList = (card.getAttribute("data-streaming") || "")
      .split(",")
      .map(s => s.trim())
      .filter(Boolean);
    svcList.forEach(svc => availableSet.add(svc));
  });

  const container = document.getElementById("streaming-filters");
  container.innerHTML = "";

  availableStreamingServices.forEach(service => {
    const wrapper  = document.createElement("div");
    const checkbox = document.createElement("input");
    checkbox.type  = "checkbox";
    checkbox.id    = `streaming-${service}`;
    checkbox.value = service;

    if (!availableSet.has(service)) {
      checkbox.disabled = true;
      wrapper.classList.add("text-muted", "opacity-50");
    }
    if (selectedServices.includes(service)) {
      checkbox.checked = true;
    }

    const label = document.createElement("label");
    label.htmlFor    = checkbox.id;
    label.textContent = service;

    wrapper.append(checkbox, label);
    container.appendChild(wrapper);
  });

  // update selectedServices & re-filter on change
  container.addEventListener("change", () => {
    selectedServices = Array.from(
      container.querySelectorAll("input[type='checkbox']:checked")
    ).map(cb => cb.value);
    filterContentByStreaming();
  });
}


function clearFilters(isPageLoad = false) {
    // Reset all filters
    document.getElementById("sortBy").value = "default";
    const minSlider = document.getElementById("minYear");
    const maxSlider = document.getElementById("maxYear");
    minSlider.value = minSlider.min;
    maxSlider.value = maxSlider.max;
    updateMinYearLabel();
    updateMaxYearLabel();

    const minRating = document.getElementById("minRating");
    const maxRating = document.getElementById("maxRating");
    minRating.value = minRating.min;
    maxRating.value = maxRating.max;
    updateMinRatingLabel();
    updateMaxRatingLabel();

    // Uncheck all the genre checkboxes in the off-canvas
    const genreCheckboxes = document.querySelectorAll("#genre-filters input[type='checkbox']");
    genreCheckboxes.forEach(cb => {
        cb.checked = false; // uncheck all genre checkboxes
    });

    // Uncheck all the streaming service checkboxes in the off-canvas
    const streamingCheckboxes = document.querySelectorAll("#streaming-filters input[type='checkbox']");
    streamingCheckboxes.forEach(cb => {
        cb.checked = false; // uncheck all streaming service checkboxes
    });

    document.getElementById("sortBy").value = "default";
    const sortDropdownButton = document.getElementById("sortGenreDropdown");
    sortDropdownButton.textContent = "Sort by";

    // Hide the clear filters button when filters are cleared
    document.getElementById("clearFilters").style.display = "none";

    // Clear the selectedGenres and selectedServices, then store them again in localStorage
    selectedGenres = new Set();  // Reinitialize the Set for genres
    selectedServices = [];       // Clear the selected services
    localStorage.setItem("selectedGenres", JSON.stringify(Array.from(selectedGenres)));
    localStorage.setItem("selectedServices", JSON.stringify(selectedServices));  // Store selectedServices

    // Skip searchMovies if this is a page load
    if (isPageLoad) return;

    // Re-trigger the search and force every card to be shown
    searchMovies().then(() => {
        // Once the search is done, update the genres again
        document.querySelectorAll('.movie-card').forEach(card => {
            // clear any inline display:none or block from prior filters
            card.style.display = '';
        });

        // Get the available genres from the search result (or set to master genres list)
        const availableGenresSet = new Set();
        document.querySelectorAll(".movie-card").forEach(card => {
            const genres = card.getAttribute("data-genres") || "";
            genres.split(",").forEach(genre => availableGenresSet.add(genre.trim()));
        });

        // Reset the genre and streaming filters in the off-canvas after clearing
        setupGenreFilter(allGenres, Array.from(availableGenresSet)); // Pass available genres here
        updateStreamingFilters();  // Reset the streaming filters

        // Update the Clear Filters button visibility
        updateClearFiltersVisibility();
    });
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
  setupGenreFilter(allGenres, allGenres);

  // When attaching event listeners for "sortBy", check if the element exists
  let sortByElem = document.getElementById("sortBy");
  if (sortByElem) {
    sortByElem.addEventListener("change", updateClearFiltersVisibility);
    sortByElem.addEventListener("click", handleFilterInteraction);
  }
  const minRatingSlider = document.getElementById("minRating");
  const maxRatingSlider = document.getElementById("maxRating");

  if (minRatingSlider && maxRatingSlider) {
    // 1) Default them if empty
    if (!minRatingSlider.value) minRatingSlider.value = minRatingSlider.min;
    if (!maxRatingSlider.value) maxRatingSlider.value = maxRatingSlider.max;

    // 2) Initial textbox sync + Clear-Filters visibility
    updateMinRatingLabel();
    updateMaxRatingLabel();
    updateClearFiltersVisibility();

    // 3) On slider “change”, mirror → show Clear → re-search
    const onRatingChange = () => {
      updateMinRatingLabel();
      updateMaxRatingLabel();
      updateClearFiltersVisibility();
      searchMovies();
    };
    minRatingSlider.addEventListener("change", onRatingChange);
    maxRatingSlider.addEventListener("change", onRatingChange);

    minRatingSlider.addEventListener("change", updateClearFiltersVisibility);
    maxRatingSlider.addEventListener("change", updateClearFiltersVisibility);
  }

  document.getElementById("genre-filters").addEventListener("change", applyAllFilters);
  document.getElementById("streaming-filters").addEventListener("change", applyAllFilters);
  document.getElementById("minYear").addEventListener("change", applyAllFilters);
  document.getElementById("maxYear").addEventListener("change", applyAllFilters);
  document.getElementById("minRating").addEventListener("change", applyAllFilters);
  document.getElementById("maxRating").addEventListener("change", applyAllFilters);
  document.getElementById("letter-filter-dropdown").addEventListener("change", applyAllFilters);

  // Attach event listener for the Clear Filters button
  document.getElementById("clearFilters").addEventListener("click", () => clearFilters());
});


if (typeof module !== 'undefined' && module.exports) {
  module.exports = { searchMovies };
}

const minBox = document.getElementById("minYearTextBox");
if (minBox) {
  minBox.addEventListener("change", updateMinYearFromTextBox);
  minBox.addEventListener("keydown", e => {
    if (e.key === "Enter") {
      e.preventDefault();
      updateMinYearFromTextBox();
    }
  });
}

const maxBox = document.getElementById("maxYearTextBox");
if (maxBox) {
  maxBox.addEventListener("change", updateMaxYearFromTextBox);
  maxBox.addEventListener("keydown", e => {
    if (e.key === "Enter") {
      e.preventDefault();
      updateMaxYearFromTextBox();
    }
  });
}

function updateMinRatingLabel() {
  const slider = document.getElementById("minRating");
  const textbox = document.getElementById("minRatingTextBox");
  if (slider && textbox) {
    textbox.value = slider.value;
  }
}

function updateMaxRatingLabel() {
  const slider = document.getElementById("maxRating");
  const textbox = document.getElementById("maxRatingTextBox");
  if (slider && textbox) {
    textbox.value = slider.value;
  }
}

function updateMinRatingFromTextBox() {
  const slider = document.getElementById("minRating");
  const box    = document.getElementById("minRatingTextBox");
  if (!slider || !box) return;

  let val = parseFloat(box.value);
  const min = parseFloat(slider.min), max = parseFloat(slider.max);

  if (isNaN(val)) {
    box.value = slider.value;
    return;
  }
  val = Math.min(max, Math.max(min, val));
  slider.value = val;
  box.value    = val;

  updateClearFiltersVisibility();
  if (!searchExecuted) {
    handleFilterInteraction({ target: slider, preventDefault: () => {} });
  } else {
    searchMovies();
  }
}

function updateMaxRatingFromTextBox() {
  const slider = document.getElementById("maxRating");
  const box    = document.getElementById("maxRatingTextBox");
  if (!slider || !box) return;

  let val = parseFloat(box.value);
  const min = parseFloat(slider.min), max = parseFloat(slider.max);

  if (isNaN(val)) {
    box.value = slider.value;
    return;
  }
  val = Math.min(max, Math.max(min, val));
  slider.value = val;
  box.value    = val;

  updateClearFiltersVisibility();
  if (!searchExecuted) {
    handleFilterInteraction({ target: slider, preventDefault: () => {} });
  } else {
    searchMovies();
  }
}

const minRatingBox = document.getElementById("minRatingTextBox");
if (minRatingBox) {
  minRatingBox.addEventListener("change", updateMinRatingFromTextBox);
  minRatingBox.addEventListener("keydown", e => {
    if (e.key === "Enter") {
      e.preventDefault();
      updateMinRatingFromTextBox();
    }
  });
}

const maxRatingBox = document.getElementById("maxRatingTextBox");
if (maxRatingBox) {
  maxRatingBox.addEventListener("change", updateMaxRatingFromTextBox);
  maxRatingBox.addEventListener("keydown", e => {
    if (e.key === "Enter") {
      e.preventDefault();
      updateMaxRatingFromTextBox();
    }
  });
}

// Dynamically generate letter filter dropdown options based on available movies
function setupLetterFilterDropdown() {
    const letterFilterDropdown = document.getElementById("letter-filter-dropdown");
    if (!letterFilterDropdown) return;

    // Clear existing options
    letterFilterDropdown.innerHTML = '<option value="" selected>All</option>';

    // Build a Set of available starting letters from the movie titles
    const availableLetters = new Set();
    const movieCards = document.querySelectorAll(".movie-card");
    movieCards.forEach(card => {
        const title = card.querySelector("h5").textContent.trim();
        if (title) {
            availableLetters.add(title[0].toUpperCase());
        }
    });

    // Generate options only for available letters
    Array.from(availableLetters)
        .sort() // Sort letters alphabetically
        .forEach(letter => {
            const option = document.createElement("option");
            option.value = letter;
            option.textContent = letter;
            letterFilterDropdown.appendChild(option);
        });

    // Add event listener to apply the filter when the dropdown value changes
    letterFilterDropdown.addEventListener("change", () => {
        const selectedLetter = letterFilterDropdown.value;
        if (selectedLetter) {
            applyLetterFilter(selectedLetter);
        } else {
            clearLetterFilter();
        }
        updateClearFiltersVisibility();
    });
}

// Clear the letter filter
function clearLetterFilter() {
    const resultsContainer = document.getElementById("results");
    const movieCards = resultsContainer.querySelectorAll(".movie-card");

    movieCards.forEach(card => {
        card.style.display = "block";
    });

    // Remove any "No movies to display" message
    const errorMessage = resultsContainer.querySelector(".error-message");
    if (errorMessage) {
        errorMessage.remove();
    }

    // Reset the dropdown to "All"
    const letterFilterDropdown = document.getElementById("letter-filter-dropdown");
    if (letterFilterDropdown) {
        letterFilterDropdown.value = "";
    }
}


document.addEventListener("DOMContentLoaded", () => {
    const sortOptions = document.querySelectorAll(".dropdown-item.sort-option");
    const sortByInput = document.getElementById("sortBy");
    const sortDropdownButton = document.getElementById("sortGenreDropdown");
    setupLetterFilterDropdown();
    sortOptions.forEach(option => {
        option.addEventListener("click", (event) => {
            event.preventDefault();
            const sortValue = option.getAttribute("data-sort");
            const sortText = option.textContent;

            // Update the hidden input value
            sortByInput.value = sortValue;

            // Update the dropdown button text
            sortDropdownButton.textContent = sortText;

            // Trigger the search with the new sorting option
            searchMovies();
        });
    });
});

function applyAllFilters() {
    const resultsContainer = document.getElementById("results");
    const movieCards = resultsContainer.querySelectorAll(".movie-card");

    // Get active filters
    const selectedGenres = Array.from(
        document.querySelectorAll("#genre-filters input[type='checkbox']:checked")
    ).map(cb => cb.value);

    const selectedServices = Array.from(
        document.querySelectorAll("#streaming-filters input[type='checkbox']:checked")
    ).map(cb => cb.value);

    const minYear = parseInt(document.getElementById("minYear").value, 10);
    const maxYear = parseInt(document.getElementById("maxYear").value, 10);

    const minRating = parseFloat(document.getElementById("minRating").value);
    const maxRating = parseFloat(document.getElementById("maxRating").value);

    const selectedLetter = document.getElementById("letter-filter-dropdown").value;

    let hasVisibleCards = false;

    // Filter movie cards
    movieCards.forEach(card => {
        const genresAttr = card.getAttribute("data-genres") || "";
        const itemGenres = genresAttr.split(",").map(s => s.trim());

        const servicesAttr = card.getAttribute("data-streaming") || "";
        const itemServices = servicesAttr.split(",").map(s => s.trim());

        const releaseYear = parseInt(card.querySelector(".movie-year").textContent.replace(/[()]/g, ""), 10);
        const rating = parseFloat(card.querySelector(".movie-rating").textContent.replace("Rating: ", ""));

        const title = card.querySelector("h5").textContent.trim();

        // Check if the card matches all active filters
        const matchesGenres = selectedGenres.length === 0 || selectedGenres.every(genre => itemGenres.includes(genre));
        const matchesServices = selectedServices.length === 0 || selectedServices.every(service => itemServices.includes(service));
        const matchesYear = (!isNaN(minYear) && !isNaN(maxYear)) ? (releaseYear >= minYear && releaseYear <= maxYear) : true;
        const matchesRating = (!isNaN(minRating) && !isNaN(maxRating)) ? (rating >= minRating && rating <= maxRating) : true;
        const matchesLetter = selectedLetter === "" || title.startsWith(selectedLetter);

        // Show or hide the card based on the filters
        if (matchesGenres && matchesServices && matchesYear && matchesRating && matchesLetter) {
            card.style.display = "block";
            hasVisibleCards = true;
        } else {
            card.style.display = "none";
        }
    });

    // Display an error message if no cards are visible
    const errorMessage = document.getElementById("noResultsMessage");
    if (!hasVisibleCards) {
        // Hide all cards explicitly
        movieCards.forEach(card => {
            card.style.display = "none";
        });

        // Show the error message
        if (!errorMessage) {
            const errorDiv = document.createElement("div");
            errorDiv.id = "noResultsMessage";
            errorDiv.className = "no-results"; // Match the existing error message class
            errorDiv.role = "alert";
            errorDiv.textContent = "No results match your selected filters. Please adjust your filters and try again.";
            resultsContainer.appendChild(errorDiv);
        }
    } else {
        // Remove the error message if results are found
        if (errorMessage) {
            errorMessage.remove();
        }
    }

    updateClearFiltersVisibility();
}