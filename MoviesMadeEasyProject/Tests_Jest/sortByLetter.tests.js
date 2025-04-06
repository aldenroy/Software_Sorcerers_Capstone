/**
 * @jest-environment jsdom
 */

const { searchMovies } = require("../MoviesMadeEasy/wwwroot/js/movieSearch");
require("@testing-library/jest-dom");

describe("Movie querying", () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <input id="searchInput" value="Avengers" />
      <select id="sortBy">
        <option value="default">Sort by</option>
        <option value="titleAsc">Title (A-Z)</option>
        <option value="titleDesc">Title (Z-A)</option>
      </select>
      <div id="results"></div>
      <div id="loadingSpinner"></div>
      <div id="genre-filters"></div>
      <div id="streaming-filters"></div>
      <button id="clearFilters" style="display: none;"></button>
      <input id="minYear" />
      <input id="maxYear" />
    `;
  });

  // Clean up after each test
  afterEach(() => {
    jest.clearAllMocks();
    document.getElementById("results").innerHTML = ""; // Clear results
  });

  function decodeHtmlEntities(html) {
    const textArea = document.createElement("textarea");
    textArea.innerHTML = html;
    return textArea.value;
  }

  test("movies are sorted by title in ascending order", async () => {
    // Mock fetch to return a response sorted by title in ascending order
    global.fetch = jest.fn(() =>
      Promise.resolve({
        json: () =>
          Promise.resolve([
            { title: "Avengers Confidential: Black Widow & Punisher", releaseYear: 2014, genres: ["Action", "Animation"], rating: 57 },
            { title: "Avengers from Hell", releaseYear: 1981, genres: ["Horror"], rating: 49 },
            { title: "Avengers Grimm", releaseYear: 2015, genres: ["Action", "Adventure", "Fantasy"], rating: 29 },
          ]),
      })
    );

    // Set the sort option to "titleAsc"
    document.getElementById("sortBy").value = "titleAsc";

    // Call the searchMovies function
    await searchMovies();

    // Get the rendered movie titles
    const renderedTitles = Array.from(document.querySelectorAll(".movie-card h5")).map((el) => el.textContent.trim());

    // Expected order after sorting
    const expectedTitles = [
      "Avengers Confidential: Black Widow & Punisher (2014)",
      "Avengers from Hell (1981)",
      "Avengers Grimm (2015)",
    ];

    // Assert that the movies are sorted correctly
    expect(renderedTitles).toEqual(expectedTitles);
  });

  test("movies are sorted by title in descending order", async () => {
    // Mock fetch to return a response sorted by title in descending order
    global.fetch = jest.fn(() =>
      Promise.resolve({
        json: () =>
          Promise.resolve([
            { title: "Avengers Grimm", releaseYear: 2015, genres: ["Action", "Adventure", "Fantasy"], rating: 29 },
            { title: "Avengers from Hell", releaseYear: 1981, genres: ["Horror"], rating: 49 },
            { title: "Avengers Confidential: Black Widow & Punisher", releaseYear: 2014, genres: ["Action", "Animation"], rating: 57 },
          ]),
      })
    );

    // Set the sort option to "titleDesc"
    document.getElementById("sortBy").value = "titleDesc";

    // Call the searchMovies function
    await searchMovies();

    // Get the rendered movie titles
    const renderedTitles = Array.from(document.querySelectorAll(".movie-card h5")).map((el) => el.textContent.trim());

    // Expected order after sorting
    const expectedTitles = [
      "Avengers Grimm (2015)",
      "Avengers from Hell (1981)",
      "Avengers Confidential: Black Widow & Punisher (2014)",
    ];

    // Assert that the movies are sorted correctly
    expect(renderedTitles).toEqual(expectedTitles);
  });

  test("empty query shows error message", async () => {
    // Set the search input to empty
    document.getElementById("searchInput").value = "";

    // Call the searchMovies function
    await searchMovies();

    // Get the results container content
    const results = decodeHtmlEntities(document.getElementById("results").innerHTML);

    // Assert that the error message is displayed
    expect(results).toContain("Please enter a movie title before searching.");
  });

  test("error handling for fetch failure", async () => {
    // Mock fetch to simulate an error
    global.fetch = jest.fn(() => Promise.reject(new Error("Failed to fetch")));

    // Call the searchMovies function
    await searchMovies();

    // Get the results container content
    const results = decodeHtmlEntities(document.getElementById("results").innerHTML);

    // Assert that the error message is displayed
    expect(results).toContain("An error occurred while fetching data. Please try again later.");
  });

  test("no results found message", async () => {
    // Mock fetch to return an empty array
    global.fetch = jest.fn(() =>
      Promise.resolve({
        json: () => Promise.resolve([]),
      })
    );

    // Call the searchMovies function
    await searchMovies();

    // Get the results container content
    const results = decodeHtmlEntities(document.getElementById("results").innerHTML);

    // Assert that the "no results" message is displayed
    expect(results).toContain("No results found.");
  });
});