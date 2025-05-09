/**
 * @jest-environment jsdom
 */
require('@testing-library/jest-dom');

describe('Movie Capture: button text & reset', () => {
  let movieModal, poster, title, genres, rating, overview, streaming, btn;
  let timeouts;

  beforeEach(() => {
    jest.resetModules();
    timeouts = [];

    global.setTimeout = (cb, ms) => timeouts.push({ cb, ms });

    document.body.innerHTML = `
      <div id="movieModal"></div>
      <h5 id="modalTitle">Test Movie (2020)</h5>
      <img id="modalPoster" src="" />
      <p id="modalGenres">Genres: Action, Adventure</p>
      <p id="modalRating">Rating: R</p>
      <p id="modalOverview">Overview: Testing capture.</p>
      <div id="modalStreaming">
        <img class="streaming-icon" alt="Netflix" />
        <img class="streaming-icon" alt="Hulu" />
      </div>
      <button class="btn-primary">View Details</button>
    `;

    movieModal = document.getElementById('movieModal');
    poster     = document.getElementById('modalPoster');
    title      = document.getElementById('modalTitle');
    genres     = document.getElementById('modalGenres');
    rating     = document.getElementById('modalRating');
    overview   = document.getElementById('modalOverview');
    streaming  = document.getElementById('modalStreaming');
    btn        = document.querySelector('button.btn-primary');

    global.fetch = jest.fn();

    require('../MoviesMadeEasy/wwwroot/js/movieCapture.js');
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  it('shows “Saved!” on success and restores text after 1.5s', async () => {
    fetch.mockResolvedValue({});

    poster.src = 'something.jpg';

    btn.click();
    movieModal.dispatchEvent(new Event('shown.bs.modal'));
    await Promise.resolve();
    await Promise.resolve();

    expect(btn.textContent).toBe('Saved!');
    expect(timeouts).toHaveLength(1);
    expect(timeouts[0].ms).toBe(1500);

    timeouts[0].cb();
    expect(btn.disabled).toBe(false);
    expect(btn.textContent).toBe('View Details');
  });

  it('shows “Error” on failure and restores text after 1.5s', async () => {
    fetch.mockRejectedValue(new Error('fail'));

    poster.src = 'something.jpg';
    btn.click();
    movieModal.dispatchEvent(new Event('shown.bs.modal'));

    await Promise.resolve();
    await Promise.resolve();

    expect(btn.textContent).toBe('Error');
    expect(timeouts).toHaveLength(1);
    expect(timeouts[0].ms).toBe(1500);

    timeouts[0].cb();
    expect(btn.disabled).toBe(false);
    expect(btn.textContent).toBe('View Details');
  });
});
