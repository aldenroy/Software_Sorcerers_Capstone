/**
 * @jest-environment jsdom
 */

const fs = require('fs');
require("@testing-library/jest-dom");

describe('dashboardPriceToggle.js', () => {

  describe('when there are prices and a button', () => {
    let btn, prices;

    beforeEach(() => {
      jest.resetModules();  
      document.body.innerHTML = `
        <button id="toggle-prices-btn" class="btn btn-secondary mb-3 d-block mx-auto">
          Show Prices
        </button>
        <div class="subscription-price d-none">\$5.00</div>
        <div class="subscription-price d-none">\$7.00</div>
      `;

      require("../MoviesMadeEasy/wwwroot/js/dashboardPriceToggle.js");

      btn    = document.getElementById('toggle-prices-btn');
      prices = Array.from(document.querySelectorAll('.subscription-price'));
    });

    test('initial state: prices hidden & button shows "Show Prices"', () => {
      expect(btn.textContent.trim()).toBe('Show Prices');
      prices.forEach(div => {
        expect(div).toHaveClass('d-none');
      });
    });

    test('click once: prices visible & button text becomes "Hide Prices"', () => {
      btn.click();
      expect(btn.textContent.trim()).toBe('Hide Prices');
      prices.forEach(div => {
        expect(div).not.toHaveClass('d-none');
      });
    });

    test('click twice: prices hidden again & button text returns to "Show Prices"', () => {
      btn.click();
      btn.click();
      expect(btn.textContent.trim()).toBe('Show Prices');
      prices.forEach(div => {
        expect(div).toHaveClass('d-none');
      });
    });
  });

  describe('when there should NOT be a toggle button', () => {
    beforeEach(() => {
      jest.resetModules();
      document.body.innerHTML = `
        <div class="subscription-price d-none">\$0.00</div>
        <div class="subscription-price d-none">\$0.00</div>
      `;
      require("../MoviesMadeEasy/wwwroot/js/dashboardPriceToggle.js");
    });

    test('script does not throw and button remains absent', () => {
      expect(document.getElementById('toggle-prices-btn')).toBeNull();
    });

    test('existing price divs stay hidden', () => {
      document.querySelectorAll('.subscription-price').forEach(div => {
        expect(div).toHaveClass('d-none');
      });
    });
  });
});
