//Audence Rating Filter
const { searchMovies } = require("../MoviesMadeEasy/wwwroot/js/movieSearch");

describe("Audience Rating Filter", () => {
  let searchInput, resultsContainer, sortBy, loadingSpinner;
  let originalLocation;

  beforeEach(() => {
    // Save the original location object.
    originalLocation = window.location;

    // Override window.location with a custom object so we can mock reload.
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: { ...originalLocation, reload: jest.fn() }
    });

    document.body.innerHTML = `
      <input type="text" id="searchInput" value="Test Query" />
      <div id="results"></div>
      <div id="loadingSpinner" style="display: none;"></div>
      <select id="sortBy">
        <option value="">Default</option>
        <option value="ratingHighLow">Rating: High to Low</option>
        <option value="ratingLowHigh">Rating: Low to High</option>
      </select>
      <input type="number" id="minYear" value="" />
      <input type="number" id="maxYear" value="" />
    `;
    searchInput = document.getElementById("searchInput");
    resultsContainer = document.getElementById("results");
    sortBy = document.getElementById("sortBy");
    loadingSpinner = document.getElementById("loadingSpinner");

    // Clear any previous fetch mocks.
    global.fetch = jest.fn();
  });

  afterEach(() => {
    // Restore the original window.location.
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: originalLocation
    });
    jest.clearAllMocks();
  });

  function extractRatingsFromResults() {
    // Expects each movie card to have a <p class="movie-rating">Rating: X</p>
    const ratingElements = resultsContainer.querySelectorAll(".movie-rating");
    return Array.from(ratingElements)
      .map(el => {
        const match = el.textContent.match(/Rating:\s*([\d.]+)/);
        return match ? parseFloat(match[1]) : null;
      })
      .filter(r => r !== null);
  }

  test("Users can filter search results by rating from high to low", async () => {
    sortBy.value = "ratingHighLow";
    // Provide movies whose rendered ratings will match the expected descending order.
    const movies = [
      { title: "Movie A", releaseYear: 2010, rating: 50 },
      { title: "Movie B", releaseYear: 2012, rating: 25 },
      { title: "Movie C", releaseYear: 2008, rating: 20 }
    ];
    global.fetch.mockResolvedValueOnce({
      json: async () => movies
    });

    await searchMovies();

    const ratings = extractRatingsFromResults();
    // Expected descending order from mock data: 50, 25, 20
    const expectedDescending = [...ratings].sort((a, b) => b - a);
    expect(ratings).toEqual(expectedDescending);
    expect(window.location.reload).not.toHaveBeenCalled();
  });

  test("Users can filter search results by rating from low to high", async () => {
    sortBy.value = "ratingLowHigh";
    // Provide movies for ascending order test.
    const movies = [
      { title: "Movie X", releaseYear: 2015, rating: 6.5 },
      { title: "Movie Y", releaseYear: 2016, rating: 7 },
      { title: "Movie Z", releaseYear: 2014, rating: 8.5 }
    ];
    global.fetch.mockResolvedValueOnce({
      json: async () => movies
    });

    await searchMovies();

    const ratings = extractRatingsFromResults();
    // Expected ascending order from mock data: 6.5, 7, 8.5
    const expectedAscending = [...ratings].sort((a, b) => a - b);
    expect(ratings).toEqual(expectedAscending);
    expect(window.location.reload).not.toHaveBeenCalled();
  });
});