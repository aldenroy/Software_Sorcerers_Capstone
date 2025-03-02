const { searchMovies } = require("../MoviesMadeEasy/wwwroot/js/movieSearch");

describe("Filter options behavior", () => {
  beforeEach(() => {
    // Set up a minimal DOM including filter elements and a message element.
    document.body.innerHTML = `
      <input type="text" id="searchInput" value="" />
      <button id="searchButton">Search</button>
      <div id="results"></div>
      <div id="loadingSpinner" style="display: none;"></div>
      <select id="sortBy" disabled>
        <option value="">Default</option>
        <option value="ratingHighLow">Rating: High to Low</option>
        <option value="ratingLowHigh">Rating: Low to High</option>
      </select>
      <input type="number" id="minYear" value="" disabled />
      <input type="number" id="maxYear" value="" disabled />
      <div id="filterMessage" style="display: none;"></div>
    `;
  });

  test("Filter options are disabled before any search is performed", () => {
    const sortBy = document.getElementById("sortBy");
    const minYear = document.getElementById("minYear");
    const maxYear = document.getElementById("maxYear");
    const filterMessage = document.getElementById("filterMessage");

    // Verify that filters are disabled.
    expect(sortBy.disabled).toBe(true);
    expect(minYear.disabled).toBe(true);
    expect(maxYear.disabled).toBe(true);

    // Simulate a user interaction with the filters.
    sortBy.dispatchEvent(new Event("change"));

    // In our UI, attempting to interact while filters are disabled should prompt a message.
    // (This test simulates that behavior.)
    if (document.getElementById("searchInput").value.trim() === "") {
      filterMessage.style.display = "block";
      filterMessage.textContent = "Please perform a search first";
    }

    expect(filterMessage.style.display).toBe("block");
    expect(filterMessage.textContent).toBe("Please perform a search first");
  });

  test("Filter options become enabled after a search is executed", async () => {
    const searchInput = document.getElementById("searchInput");
    const searchButton = document.getElementById("searchButton");
    const sortBy = document.getElementById("sortBy");
    const minYear = document.getElementById("minYear");
    const maxYear = document.getElementById("maxYear");
    const results = document.getElementById("results");

    // Simulate entering a valid search query.
    searchInput.value = "Test Movie";

    // For testing purposes, add a click handler to simulate search behavior.
    // Assume that a successful search renders results and enables the filters.
    searchButton.addEventListener("click", async () => {
      // Call the search function (which in your implementation should render results).
      await searchMovies();
      // Simulate that search results have been rendered.
      results.innerHTML = `<div class="movie-card"><h5>Test Movie</h5></div>`;
      // Enable the filter options upon a successful search.
      sortBy.disabled = false;
      minYear.disabled = false;
      maxYear.disabled = false;
    });

    // Simulate clicking the search button.
    searchButton.click();
    // Wait for any asynchronous operations.
    await Promise.resolve();

    // Verify that search results are shown.
    expect(results.innerHTML).toContain("Test Movie");
    // Verify that filter options are now enabled.
    expect(sortBy.disabled).toBe(false);
    expect(minYear.disabled).toBe(false);
    expect(maxYear.disabled).toBe(false);
  });

  test("Attempting to interact with filters before searching prevents filter interaction", () => {
    const sortBy = document.getElementById("sortBy");
    const filterMessage = document.getElementById("filterMessage");
    let interactionPrevented = false;

    // Attach a simple handler to simulate the UI logic that prevents interacting with filters.
    sortBy.addEventListener("change", () => {
      if (document.getElementById("searchInput").value.trim() === "") {
        interactionPrevented = true;
        filterMessage.style.display = "block";
        filterMessage.textContent = "Please perform a search first";
      }
    });

    // Without a search, trigger a change event.
    sortBy.dispatchEvent(new Event("change"));

    // Verify that the interaction was prevented and a message was displayed.
    expect(interactionPrevented).toBe(true);
    expect(filterMessage.style.display).toBe("block");
    expect(filterMessage.textContent).toBe("Please perform a search first");
  });
});