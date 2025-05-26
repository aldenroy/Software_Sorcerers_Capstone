/**
 * @jest-environment jsdom
 */


// loading.tests.js
const { searchMovies } = require("../MoviesMadeEasy/wwwroot/js/movieSearch");
require("@testing-library/jest-dom");

global.fetch = jest.fn();

describe("searchMovies function", () => {
    let searchInput, resultsContainer, loadingSpinner, sortBy, minYear, maxYear;

    //set the index html code
    beforeEach(() => {
    document.body.innerHTML = `
        <input type="text" id="searchInput" />
        <div id="results"></div>
        <div id="loadingSpinner" style="display: none;"></div>
        <select id="sortBy">
            <option value="default">Sort by</option>
            <option value="yearAsc">Year Ascending</option>
            <option value="yearDesc">Year Descending</option>
        </select>
        <input type="number" id="minYear" />
        <input type="number" id="maxYear" />
        
        <div id="genre-filters">
            <label><input type="checkbox" value="Action" checked />Action</label>
            <label><input type="checkbox" value="Drama" />Drama</label>
        </div>
    `;

        searchInput = document.getElementById("searchInput");
        resultsContainer = document.getElementById("results");
        loadingSpinner = document.getElementById("loadingSpinner");
        sortBy = document.getElementById("sortBy");
        minYear = document.getElementById("minYear");
        maxYear = document.getElementById("maxYear");

        fetch.mockClear();
    });

    //Tests for searching inception and checks if it shows the updated display
    test("shows the loading spinner when search starts", async () => {
        fetch.mockResolvedValueOnce({
            json: jest.fn().mockResolvedValueOnce([])
        });
    
        searchInput.value = "Inception";
    
        // Trigger the search and immediately check spinner visibility
        const searchPromise = searchMovies();
    
        // Check that the spinner is visible during the search
        expect(loadingSpinner.style.display).toBe("block");
    
        await searchPromise;
    });
    

    test("hides the loading spinner after fetching results", async () => {
        fetch.mockResolvedValueOnce({
            json: jest.fn().mockResolvedValueOnce([{ title: "Inception", releaseYear: 2010 }])
        });

        searchInput.value = "Inception";
        await searchMovies();

        expect(loadingSpinner.style.display).toBe("none");
    });

    test("hides the loading spinner if an error occurs", async () => {
        fetch.mockRejectedValueOnce(new Error("Network error"));

        searchInput.value = "Inception";
        await searchMovies();

        expect(loadingSpinner.style.display).toBe("none");
    });

    test("does not show the spinner if the search input is empty", async () => {
        searchInput.value = "";
        await searchMovies();

        expect(loadingSpinner.style.display).toBe("none");
    });
});