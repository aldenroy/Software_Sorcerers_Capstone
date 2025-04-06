const React = require('react');
const { render, screen, fireEvent, waitFor } = require('@testing-library/react');
require('@testing-library/jest-dom');

// A simple mocked ContentBrowsingPage component
function ContentBrowsingPage() {
  const streamingServices = ["Netflix", "Hulu", "Amazon Prime"];
  const content = [
    { id: 1, title: 'Movie A', service: 'Netflix' },
    { id: 2, title: 'Movie B', service: 'Hulu' },
    { id: 3, title: 'Movie C', service: 'Amazon Prime' },
    { id: 4, title: 'Movie D', service: 'Netflix' },
  ];
  
  const [filters, setFilters] = React.useState([]);
  
  const toggleFilter = (service) => {
    setFilters((prev) =>
      prev.includes(service) ? prev.filter(s => s !== service) : [...prev, service]
    );
  };
  
  const clearFilters = () => setFilters([]);
  
  const filteredContent = filters.length
    ? content.filter(item => filters.includes(item.service))
    : content;
  
  return React.createElement('div', null,
    // Display available streaming service filters as buttons
    React.createElement('div', { 'data-testid': 'streaming-services' },
      streamingServices.map(service =>
        React.createElement('button', {
          key: service,
          onClick: () => toggleFilter(service),
          'data-testid': `filter-${service}`
        }, service)
      )
    ),
    // Clear Filters Button
    React.createElement('button', { onClick: clearFilters, 'data-testid': 'clear-filters' }, 'Clear Filters'),
    // List content items
    React.createElement('div', { 'data-testid': 'content-list' },
      filteredContent.map(item =>
        React.createElement('div', { key: item.id, 'data-testid': 'content-item' }, item.title)
      )
    )
  );
}

describe('Content Filtering Features', () => {
  test('Filtering content by a single streaming service', () => {
    render(React.createElement(ContentBrowsingPage));
    
    // Assert streaming service buttons are displayed
    const netflixButton = screen.getByTestId('filter-Netflix');
    expect(netflixButton).toBeInTheDocument();
    
    // User selects "Netflix"
    fireEvent.click(netflixButton);
    
    // Assert only Netflix items are displayed ("Movie A" and "Movie D")
    const contentItems = screen.getAllByTestId('content-item').map(item => item.textContent);
    expect(contentItems).toEqual(expect.arrayContaining(['Movie A', 'Movie D']));
    // Ensure items from Hulu or Amazon Prime are not present
    expect(contentItems).not.toEqual(expect.arrayContaining(['Movie B', 'Movie C']));
  });
  
  test('Filtering content by multiple streaming services', () => {
    render(React.createElement(ContentBrowsingPage));
    
    const netflixButton = screen.getByTestId('filter-Netflix');
    const huluButton = screen.getByTestId('filter-Hulu');
    
    // User selects both "Netflix" and "Hulu"
    fireEvent.click(netflixButton);
    fireEvent.click(huluButton);
    
    // Assert that content items for Netflix and Hulu are present
    const contentItems = screen.getAllByTestId('content-item').map(item => item.textContent);
    expect(contentItems).toEqual(expect.arrayContaining(['Movie A', 'Movie D', 'Movie B']));
    // Ensure content not available in either service is excluded
    expect(contentItems).not.toEqual(expect.arrayContaining(['Movie C']));
  });
  
  test('Clearing applied streaming service filters', () => {
    render(React.createElement(ContentBrowsingPage));
    
    const netflixButton = screen.getByTestId('filter-Netflix');
    fireEvent.click(netflixButton);
    
    // Assert filtering is applied (only Netflix items)
    let contentItems = screen.getAllByTestId('content-item').map(item => item.textContent);
    expect(contentItems).toEqual(expect.arrayContaining(['Movie A', 'Movie D']));
    expect(contentItems).not.toEqual(expect.arrayContaining(['Movie B', 'Movie C']));
    
    // User clicks the "Clear Filters" button
    const clearButton = screen.getByTestId('clear-filters');
    fireEvent.click(clearButton);
    
    // All filters should be cleared and full list restored (4 items)
    contentItems = screen.getAllByTestId('content-item').map(item => item.textContent);
    expect(contentItems.length).toBe(4);
  });
  
  test('Filter response time performance', async () => {
    render(React.createElement(ContentBrowsingPage));
    
    const netflixButton = screen.getByTestId('filter-Netflix');
    
    // Start timing
    const start = performance.now();
    fireEvent.click(netflixButton);
    
    // Wait for UI update (in this simple example it should be immediate)
    await waitFor(() => {
      const contentItems = screen.getAllByTestId('content-item');
      expect(contentItems.length).toBe(2);
    });
    
    const end = performance.now();
    const duration = end - start;
    
    // Assert that filtering update takes less than 2000ms
    expect(duration).toBeLessThan(2000);
  });
});