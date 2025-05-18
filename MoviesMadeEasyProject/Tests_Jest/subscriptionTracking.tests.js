/**
 * @jest-environment jsdom
 */
require('../MoviesMadeEasy/wwwroot/js/subscriptionTracking.js');

describe('Subscription Click Tracker', () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <meta name="RequestVerificationToken" content="abc123">
      <div class="subscription-item" data-service-id="42">
        <a href="#" class="subscription-link">Valid subscription</a>
      </div>
      <div class="no-item">
        <a href="#" class="subscription-link">Missing subscription item</a>
      </div>
      <div class="subscription-item" data-service-id="">
        <a href="#" class="subscription-link">Empty serviceId</a>
      </div>
    `;

    global.fetch = jest.fn().mockResolvedValue({ ok: true });
    jest.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  test('should send a POST request to track the click with correct URL, headers, and body', () => {
    document.dispatchEvent(new Event('DOMContentLoaded'));

    const validLink = document.querySelector('.subscription-item .subscription-link');
    validLink.click();

    expect(global.fetch).toHaveBeenCalledWith(
      '/User/TrackSubscriptionClick',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': 'abc123'
        },
        body: JSON.stringify({ streamingServiceId: 42 })
      }
    );
  });

  test('should not attempt tracking when link is outside a .subscription-item', () => {
    document.dispatchEvent(new Event('DOMContentLoaded'));

    const missingItemLink = Array.from(document.querySelectorAll('.subscription-link'))
      .find(a => a.textContent === 'Missing subscription item');
    missingItemLink.click();

    expect(global.fetch).not.toHaveBeenCalled();
  });

  test('should not attempt tracking when data-service-id is empty', () => {
    document.dispatchEvent(new Event('DOMContentLoaded'));

    const emptyIdLink = Array.from(document.querySelectorAll('.subscription-link'))
      .find(a => a.textContent === 'Empty serviceId');
    emptyIdLink.click();

    expect(global.fetch).not.toHaveBeenCalled();
  });

  test('should log an error if the server responds with an error status', async () => {
    global.fetch.mockResolvedValueOnce({ ok: false, statusText: 'Bad Request' });

    document.dispatchEvent(new Event('DOMContentLoaded'));
    const validLink = document.querySelector('.subscription-item .subscription-link');
    validLink.click();

    await new Promise(res => setTimeout(res, 0));

    expect(console.error).toHaveBeenCalledWith(
      'Failed to record click:',
      'Bad Request'
    );
  });

  test('should log an error when the fetch request fails', async () => {
    const networkError = new Error('Network down');
    global.fetch.mockRejectedValueOnce(networkError);

    document.dispatchEvent(new Event('DOMContentLoaded'));
    const validLink = document.querySelector('.subscription-item .subscription-link');
    validLink.click();
    
    await new Promise(res => setTimeout(res, 0));

    expect(console.error).toHaveBeenCalledWith(
      'Error in tracking subscription click:',
      networkError
    );
  });
});
