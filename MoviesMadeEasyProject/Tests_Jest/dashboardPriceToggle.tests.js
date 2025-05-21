/**
 * @jest-environment jsdom
 */

const fs = require('fs');
require('@testing-library/jest-dom');

describe('dashboardPriceToggle.js', () => {

  describe('when there are prices, a total, and a button', () => {
    let btn, prices, total, analysis;

    beforeEach(() => {
      jest.resetModules();
      document.body.innerHTML = `
        <button id="toggle-analysis-btn"
                class="btn btn-secondary mb-3 d-block mx-auto">
          Streaming Service Analysis
        </button>

        <div id="subscription-total" class="d-none">
          $12.00
        </div>

        <div class="subscription-price d-none">$5.00</div>
        <div class="subscription-price d-none">$7.00</div>

        <div id="analysis-section" class="d-none">
          Usage Summary Content
        </div>
      `;

      // load and run the IIFE
      require('../MoviesMadeEasy/wwwroot/js/dashboardPriceToggle.js');

      btn    = document.getElementById('toggle-analysis-btn');
      total  = document.getElementById('subscription-total');
      prices = Array.from(document.querySelectorAll('.subscription-price'));
      analysis = document.getElementById('analysis-section');
    });

    test('initial state: prices, total & analysis hidden; button shows "Streaming Service Analysis"', () => {
      expect(btn.textContent.trim()).toBe('Streaming Service Analysis');
      expect(total).toHaveClass('d-none');
      prices.forEach(div => expect(div).toHaveClass('d-none'));
      expect(analysis).toHaveClass('d-none');
    });

    test('click once: prices, total & analysis visible; button text becomes "Hide Analysis"', () => {
      btn.click();
      expect(btn.textContent.trim()).toBe('Hide Analysis');
      expect(total).not.toHaveClass('d-none');
      prices.forEach(div => expect(div).not.toHaveClass('d-none'));
      expect(analysis).not.toHaveClass('d-none');
    });

    test('click twice: everything hidden again; button text returns to default', () => {
      btn.click();
      btn.click();
      expect(btn.textContent.trim()).toBe('Streaming Service Analysis');
      expect(total).toHaveClass('d-none');
      prices.forEach(div => expect(div).toHaveClass('d-none'));
      expect(analysis).toHaveClass('d-none');
    });
  });

  describe('when there should NOT be a toggle button', () => {
    beforeEach(() => {
      jest.resetModules();
      document.body.innerHTML = `
        <div class="subscription-price d-none">$0.00</div>
        <div class="subscription-price d-none">$0.00</div>
      `;
      require('../MoviesMadeEasy/wwwroot/js/dashboardPriceToggle.js');
    });

    test('script does not throw and button remains absent', () => {
      expect(document.getElementById('toggle-analysis-btn')).toBeNull();
    });

    test('existing price divs stay hidden', () => {
      document.querySelectorAll('.subscription-price').forEach(div => {
        expect(div).toHaveClass('d-none');
      });
    });

    test('analysis section remains absent', () => {
      expect(document.getElementById('analysis-section')).toBeNull();
    });
  });

});
