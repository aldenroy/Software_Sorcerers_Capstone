/**
 * @jest-environment jsdom
 */

require('@testing-library/jest-dom');
require('../MoviesMadeEasy/wwwroot/js/dashboardModalHelper.js');  

describe('dashboardModalHelper click delegation', () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <div class="movie-card">
        <a href="javascript:void(0)">
          <img class="img-fluid mb-2" />
          <span class="movie-title">Inception</span>
        </a>
        <button type="button" class="btn btn-primary d-none">View Details</button>
      </div>`;
  });

  test('poster click triggers hidden button', () => {
    const button = document.querySelector('button');
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
});
