let searchExecuted = true;
const NO_ITEMS_HTML = "<div class='no-results' role='alert'>No items found.</div>";
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
  const minYear = document.getElementById("minYear"),
    maxYear = document.getElementById("maxYear");
  const yearDefault =
    minYear.value === minYear.min &&
    maxYear.value === maxYear.max;

  // 3) rating default?
  const minRating = document.getElementById("minRating"),
    maxRating = document.getElementById("maxRating");
  const ratingDefault =
    minRating.value === minRating.min &&
    maxRating.value === maxRating.max;

  // 4) any genres or streaming checked?
  const genresChecked = !!document.querySelector("#genre-filters input:checked"),
    streamingChecked = !!document.querySelector("#streaming-filters input:checked");

  // Show clearBtn if *any* filter is non-default
  if (
    !sortDefault ||
    !yearDefault ||
    !ratingDefault ||
    genresChecked ||
    streamingChecked
  ) {
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

  // Re-trigger the search, then force every card to be shown
  searchMovies().then(() => {
    document.querySelectorAll('.movie-card').forEach(card => {
      // clear any inline display:none or block from prior filters
      card.style.display = '';
    });
    // update the Clear Filters button visibility one last time
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
  setupGenreFilter(allGenres);

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

  document.getElementById("minYear").addEventListener("input", updateClearFiltersVisibility);
  document.getElementById("maxYear").addEventListener("input", updateClearFiltersVisibility);

  document.getElementById("minYear").addEventListener("focus", handleFilterInteraction);
  document.getElementById("maxYear").addEventListener("focus", handleFilterInteraction);

  // Attach event listener for the Clear Filters button
  document.getElementById("clearFilters").addEventListener("click", clearFilters);
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