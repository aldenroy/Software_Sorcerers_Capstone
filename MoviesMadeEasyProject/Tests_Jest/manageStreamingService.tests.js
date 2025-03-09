/**
 * @jest-environment jsdom
 */

const { manageStreamingService } = require("../MoviesMadeEasy/wwwroot/js/manageStreamingService");
require("@testing-library/jest-dom");

function initializeModule() {
  jest.resetModules();
  require('../MoviesMadeEasy/wwwroot/js/manageStreamingService.js');
  document.dispatchEvent(new Event('DOMContentLoaded'));
}

let preSelectedValue = ""; 

function setupDOM(preSelectedVal) {
  preSelectedValue = preSelectedVal;
  document.body.innerHTML = `
    <form id="subscriptionForm">
      <input type="hidden" id="selectedServices" value="" />
      <button type="submit">Submit</button>
    </form>
    <div class="subscription-container">
      <div class="card" data-id="1"></div>
      <div class="card" data-id="2"></div>
      <div class="card" data-id="3"></div>
    </div>
    <input type="hidden" id="preSelectedServices" value="${preSelectedValue}" />
  `;
}

describe('Subscription Selection Functionality', () => {
    beforeEach(() => {
      setupDOM("");
      initializeModule();
    });
  
    afterEach(() => {
      document.body.innerHTML = '';
    });

    test('should have empty hidden input on initialization', () => {
        const hiddenInput = document.getElementById('selectedServices');
        expect(hiddenInput.value).toBe("");
    });
      
  
    test('should select a card and update the hidden input', () => {
      const card = document.querySelector('.card[data-id="1"]');
      const hiddenInput = document.getElementById('selectedServices');
  
      card.click();
      expect(card.classList.contains('selected')).toBe(true);
      expect(hiddenInput.value).toBe("1");
    });
  
    test('should deselect a card on second click and update the hidden input', () => {
      const card = document.querySelector('.card[data-id="1"]');
      const hiddenInput = document.getElementById('selectedServices');
  
      card.click();
      expect(card.classList.contains('selected')).toBe(true);
      expect(hiddenInput.value).toBe("1");
  
      card.click();
      expect(card.classList.contains('selected')).toBe(false);
      expect(hiddenInput.value).toBe("");
    });
  
    test('should update hidden input correctly when multiple cards are selected and deselected', () => {
      const card1 = document.querySelector('.card[data-id="1"]');
      const card2 = document.querySelector('.card[data-id="2"]');
      const hiddenInput = document.getElementById('selectedServices');
  
      card1.click();
      expect(hiddenInput.value).toBe("1");
  
      card2.click();
      expect(hiddenInput.value).toBe("1,2");
  
      card1.click();
      expect(hiddenInput.value).toBe("2");
    });

    test('should allow form submission when a service is selected', () => {
      window.alert = jest.fn();

      const card = document.querySelector('.card[data-id="1"]');

      card.click();
      const form = document.getElementById('subscriptionForm');
      const submitEvent = new Event('submit', { cancelable: true });
      submitEvent.preventDefault = jest.fn();

      form.dispatchEvent(submitEvent);

      expect(window.alert).not.toHaveBeenCalled();
      expect(submitEvent.preventDefault).not.toHaveBeenCalled();
    });

    test('should preselect cards based on preSelectedServices input value', () => {
      setupDOM("1,3");
      initializeModule();
  
      const hiddenInput = document.getElementById('selectedServices');
      const card1 = document.querySelector('.card[data-id="1"]');
      const card3 = document.querySelector('.card[data-id="3"]');
      const card2 = document.querySelector('.card[data-id="2"]');
  
      expect(card1.classList.contains('selected')).toBe(true);
      expect(card3.classList.contains('selected')).toBe(true);
      expect(card2.classList.contains('selected')).toBe(false);
      expect(hiddenInput.value).toBe("1,3");
    });

    test('should update hidden input correctly when toggling a preselected card', () => {
      setupDOM("1");
      initializeModule();
  
      const hiddenInput = document.getElementById('selectedServices');
      const card = document.querySelector('.card[data-id="1"]');
  
      expect(card.classList.contains('selected')).toBe(true);
      expect(hiddenInput.value).toBe("1");

      card.click();
      expect(card.classList.contains('selected')).toBe(false);
      expect(hiddenInput.value).toBe("");
    });

    test('should add a non-preselected card to preselected ones when clicked', () => {
      setupDOM("2");
      initializeModule();
  
      const hiddenInput = document.getElementById('selectedServices');
      const card1 = document.querySelector('.card[data-id="1"]');
      const card2 = document.querySelector('.card[data-id="2"]');
  
      expect(card2.classList.contains('selected')).toBe(true);
      expect(card1.classList.contains('selected')).toBe(false);
      expect(hiddenInput.value).toBe("2");
  
      card1.click();
      expect(hiddenInput.value).toBe("2,1");
    });

    test('with no preselected cards, a card should be marked for addition when selected', () => {
        const card = document.querySelector('.card[data-id="1"]');

        expect(card.classList.contains('marked-for-addition')).toBe(false);

        card.click();
        expect(card.classList.contains('selected')).toBe(true);
        expect(card.classList.contains('marked-for-addition')).toBe(true);
    });

    test('with a preselected service, a card should not show a marker until toggled', () => {
        setupDOM("1");
        initializeModule();

        const card = document.querySelector('.card[data-id="1"]');

        expect(card.classList.contains('selected')).toBe(true);
        expect(card.classList.contains('marked-for-deletion')).toBe(false);
    });

    test('when a preselected card is clicked (deselected), it should show deletion marker', () => {
        setupDOM("1");
        initializeModule();

        const card = document.querySelector('.card[data-id="1"]');

        card.click();
        expect(card.classList.contains('selected')).toBe(false);
        expect(card.classList.contains('marked-for-deletion')).toBe(true);
    });

    test('when a non-preselected card is clicked, it should show addition marker', () => {
        setupDOM("2");
        initializeModule();

        const card = document.querySelector('.card[data-id="1"]');

        card.click();
        expect(card.classList.contains('selected')).toBe(true);
        expect(card.classList.contains('marked-for-addition')).toBe(true);
    });

    test('clicking a preselected card twice should revert to original state (no deletion marker)', () => {
        setupDOM("1");
        initializeModule();

        const preCard = document.querySelector('.card[data-id="1"]');
        expect(preCard.classList.contains('marked-for-deletion')).toBe(false);

        preCard.click();
        expect(preCard.classList.contains('marked-for-deletion')).toBe(true);

        preCard.click();
        expect(preCard.classList.contains('selected')).toBe(true);
        expect(preCard.classList.contains('marked-for-deletion')).toBe(false);
    });

    test('clicking a non-preselected card twice should revert to original state (no addition marker)', () => {
        setupDOM("2");
        initializeModule();

        const nonPreCard = document.querySelector('.card[data-id="1"]');
        expect(nonPreCard.classList.contains('marked-for-addition')).toBe(false);

        nonPreCard.click();
        expect(nonPreCard.classList.contains('marked-for-addition')).toBe(true);

        nonPreCard.click();
        expect(nonPreCard.classList.contains('selected')).toBe(false);
        expect(nonPreCard.classList.contains('marked-for-addition')).toBe(false);
    });

    test('should alert "No changes were made" when submitted without changes', () => {
        setupDOM("1,2");
        initializeModule();

        window.alert = jest.fn();
        const form = document.getElementById('subscriptionForm');
        const submitEvent = new Event('submit', { cancelable: true });

        submitEvent.preventDefault = jest.fn();
        form.dispatchEvent(submitEvent);
        expect(window.alert).toHaveBeenCalledWith("No changes were made");
    });

  });
  