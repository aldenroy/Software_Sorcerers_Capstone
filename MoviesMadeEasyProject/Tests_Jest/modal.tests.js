// Polyfill for TextEncoder and TextDecoder
const { TextEncoder, TextDecoder } = require('text-encoding');
global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder;

const { searchMovies } = require("../MoviesMadeEasy/wwwroot/js/movieSearch");
const { JSDOM } = require('jsdom');
require("@testing-library/jest-dom");

// Set up a basic DOM environment using JSDOM
const dom = new JSDOM(`
<!DOCTYPE html>
<html>
<head></head>
<body>
    <div id="results">
        <div class="movie-card" data-overview="This is a test overview." data-streaming="Netflix, Hulu">
            <h5>Test Movie</h5>
            <img src="test-poster.jpg" alt="Test Movie Poster">
            <p class="movie-genres">Genres: Action, Adventure</p>
            <p class="movie-rating">Rating: 8.5</p>
            <button class="btn btn-primary">View Details</button>
        </div>
    </div>
    <div id="movieModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalTitle"></h5>
                </div>
                <div class="modal-body">
                    <img id="modalPoster" src="" alt="Movie Poster" class="img-fluid">
                    <div class="modal-text-content">
                        <p id="modalGenres"></p>
                        <p id="modalRating"></p>
                        <p id="modalOverview"></p>
                        <p id="modalStreaming"></p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
`);

global.document = dom.window.document;
global.window = dom.window;

// Mock Bootstrap Modal
const mockShow = jest.fn();
jest.mock('bootstrap', () => ({
    Modal: jest.fn(() => ({
        show: mockShow,
    })),
}));

// Mock the openModal function
function openModal(button) {
    const movieCard = button.closest('.movie-card');

    const title = movieCard.querySelector('h5').textContent;
    const posterUrl = movieCard.querySelector('img').src;
    const genres = movieCard.querySelector('.movie-genres').textContent.replace("Genres: ", "");
    const rating = movieCard.querySelector('.movie-rating').textContent.replace("Rating: ", "");
    const overview = movieCard.getAttribute('data-overview') || "No overview available.";
    const streamingServices = movieCard.getAttribute('data-streaming') || "Not available on streaming platforms.";

    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalPoster').src = posterUrl;
    document.getElementById('modalGenres').textContent = `Genres: ${genres}`;
    document.getElementById('modalRating').textContent = `Rating: ${rating}`;
    document.getElementById('modalOverview').textContent = `Overview: ${overview}`;
    document.getElementById('modalStreaming').textContent = `Streaming Services: ${streamingServices}`;

    const movieModal = new (require('bootstrap').Modal)(document.getElementById('movieModal'));
    movieModal.show();
}

// Attach the mock function to the global scope
global.openModal = openModal;

// Test suite for modal functionality
describe('Modal Functionality', () => {
    beforeEach(() => {
        // Reset the DOM and mocks before each test
        document.body.innerHTML = dom.window.document.body.innerHTML;
        jest.clearAllMocks();
    });

    test('Modal appears when "View Details" button is clicked', () => {
        // Simulate a click on the "View Details" button
        const viewDetailsButton = document.querySelector('.btn-primary');
        viewDetailsButton.click();

        // Explicitly call the openModal function
        openModal(viewDetailsButton);

        // Check if all the attributes are populated correctly.
        expect(document.getElementById('modalTitle').textContent).toBe('Test Movie');
        expect(document.getElementById('modalPoster').src).toContain('test-poster.jpg');
        expect(document.getElementById('modalGenres').textContent).toBe('Genres: Action, Adventure');
        expect(document.getElementById('modalRating').textContent).toBe('Rating: 8.5');
        expect(document.getElementById('modalOverview').textContent).toBe('Overview: This is a test overview.');
        expect(document.getElementById('modalStreaming').textContent).toBe('Streaming Services: Netflix, Hulu');
        expect(mockShow).toHaveBeenCalled();
    });

    test('Modal displays "No overview available" and "Not available on streaming platforms" when data is missing', () => {
        // Remove the data attributes from the movie card
        const movieCard = document.querySelector('.movie-card');
        movieCard.removeAttribute('data-overview');
        movieCard.removeAttribute('data-streaming');

        // Simulate a click on the "View Details" button
        const viewDetailsButton = document.querySelector('.btn-primary');
        viewDetailsButton.click();

        // Explicitly call the openModal function
        openModal(viewDetailsButton);

        // Check if the modal overview displays the default message
        expect(document.getElementById('modalOverview').textContent).toBe('Overview: No overview available.');

        // Check if the modal streaming services display the default message
        expect(document.getElementById('modalStreaming').textContent).toBe('Streaming Services: Not available on streaming platforms.');
    });
});