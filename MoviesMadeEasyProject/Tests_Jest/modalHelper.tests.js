/**
 * @jest-environment jsdom
 */

require('@testing-library/jest-dom');
require('../MoviesMadeEasy/wwwroot/js/dashboardModalHelper.js');

const flushPromises = () => new Promise(resolve => setTimeout(resolve, 0));

describe('dashboardModalHelper click delegation', () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <div class="movie-card">
        <a href="javascript:void(0)">
          <img class="img-fluid mb-2" />
          <span class="movie-title">Inception</span>
        </a>
        <button type="button" class="btn btn-primary d-none">View Details</button>
        <button
          type="button"
          class="remove-rvt"
          data-title-id="123"
        >Remove</button>
      </div>`;

    // stub fetch so .mockResolvedValue will work
    global.fetch = jest.fn();
  });

  test('poster click triggers hidden button', () => {
    const button = document.querySelector('button'); // first button is .btn-primary
    const spy = jest.fn();
    button.addEventListener('click', spy);

    document.querySelector('img')
      .dispatchEvent(new MouseEvent('click', { bubbles: true }));

    expect(spy).toHaveBeenCalledTimes(1);
  });

  test('title click triggers hidden button', () => {
    const button = document.querySelector('button');
    const spy = jest.fn();
    button.addEventListener('click', spy);

    document.querySelector('.movie-title')
      .dispatchEvent(new MouseEvent('click', { bubbles: true }));

    expect(spy).toHaveBeenCalledTimes(1);
  });

  test('click outside movie card does not trigger button', () => {
    const button = document.querySelector('button');
    const spy = jest.fn();
    button.addEventListener('click', spy);

    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));

    expect(spy).not.toHaveBeenCalled();
  });

  test('clicking remove button sends DELETE request with correct endpoint', async () => {
    global.fetch.mockResolvedValue({ ok: true });

    document.querySelector('.remove-rvt')
      .dispatchEvent(new MouseEvent('click', { bubbles: true }));

    await flushPromises();

    expect(global.fetch).toHaveBeenCalledTimes(1);
    expect(global.fetch).toHaveBeenCalledWith(
      '/User/RemoveRecentlyViewed/123',
      { method: 'DELETE' }
    );
  });

  test('successful remove deletes the card from the DOM', async () => {
    global.fetch.mockResolvedValue({ ok: true });

    document.querySelector('.remove-rvt')
      .dispatchEvent(new MouseEvent('click', { bubbles: true }));

    await flushPromises();

    expect(document.querySelector('.movie-card')).toBeNull();
  });

  test('failed remove retains the card in the DOM', async () => {
    global.fetch.mockResolvedValue({ ok: false });

    document.querySelector('.remove-rvt')
      .dispatchEvent(new MouseEvent('click', { bubbles: true }));

    await flushPromises();

    expect(document.querySelector('.movie-card')).not.toBeNull();
  });
});
