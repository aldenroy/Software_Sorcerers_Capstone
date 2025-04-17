/**
 * @jest-environment jsdom
 */

const { manageStreamingService } = require("../MoviesMadeEasy/wwwroot/js/movieCapture.js");
require("@testing-library/jest-dom");

describe('movieCapture.js', () => {
  beforeEach(() => {
    jest.resetModules();

    document.body.innerHTML = `
      <article class="movie-card"
        data-external-id="imdb123"
        data-type="MovieType"
        data-poster-url="http://poster.jpg"
        data-genres="Action,Comedy"
        data-overview="Overview text"
        data-streaming="Netflix,Hulu">
        <div class="movie-details">
          <h5>Title Name<span class="movie-year">(2025)</span></h5>
          <p class="movie-rating">Rating: 9.5</p>
          <button class="btn-view-details">View Details</button>
        </div>
      </article>
      <button id="other">Not a detail btn</button>
    `;

    global.fetch = jest.fn(() => Promise.resolve({ ok: true }));
  });

  it('does nothing when clicking a non‑.btn-view-details element', () => {
    document.getElementById('other').click();
    expect(global.fetch).not.toHaveBeenCalled();
  });

  it('POSTs the correct payload & toggles the button on success', async () => {
    jest.useFakeTimers();
    const btn = document.querySelector('.btn-view-details');

    btn.click();

    expect(btn.disabled).toBe(true);

    expect(global.fetch).toHaveBeenCalledWith(
      '/Home/CaptureMovie',
      expect.objectContaining({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ExternalId:        'imdb123',
          TitleName:         'Title Name',
          Year:              2025,
          Type:              'MovieType',
          PosterUrl:         'http://poster.jpg',
          Genres:            'Action,Comedy',
          Rating:            '9.5',
          Overview:          'Overview text',
          StreamingServices: 'Netflix,Hulu'
        })
      })
    );


    await Promise.resolve();
    expect(btn.textContent).toBe('Saved!');

    jest.advanceTimersByTime(1500);
    expect(btn.disabled).toBe(false);
    expect(btn.textContent).toBe('View Details');

    jest.useRealTimers();
  });

  it('shows “Error” on fetch rejection and still resets', async () => {
    global.fetch = jest.fn(() => Promise.reject(new Error('fail')));
    jest.useFakeTimers();
    const btn = document.querySelector('.btn-view-details');

    btn.click();
    await Promise.resolve();      
    expect(btn.textContent).toBe('Error');

    jest.advanceTimersByTime(1500);
    expect(btn.textContent).toBe('View Details');

    jest.useRealTimers();
  });
});
