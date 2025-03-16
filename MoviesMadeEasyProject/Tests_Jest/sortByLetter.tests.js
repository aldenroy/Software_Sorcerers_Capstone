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

    // Instead of comparing raw innerHTML, extract the title and year separately.
    const movieCards = Array.from(document.querySelectorAll(".movie-card h5"));
    const movieData = movieCards.map(el => {
      // Assume that the first text node is the title and the span inside has the year.
      const titleText = el.childNodes[0]?.textContent.trim() || "";
      const yearText = el.querySelector(".movie-year")?.textContent.trim() || "";
      return { title: titleText, year: yearText };
    });

    // Expected data in ascending order (titles sorted alphabetically)
    const expectedData = [
      { title: "Avengers Confidential: Black Widow & Punisher", year: "(2014)" },
      { title: "Avengers from Hell", year: "(1981)" },
      { title: "Avengers Grimm", year: "(2015)" },
    ];

    // Verify that each rendered card has the expected title and year in order.
    expectedData.forEach((expected, idx) => {
      expect(movieData[idx].title).toBe(expected.title);
      expect(movieData[idx].year).toBe(expected.year);
    });
  });
});