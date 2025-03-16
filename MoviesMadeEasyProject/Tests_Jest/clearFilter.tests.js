const React = require('react');
const { render, screen, fireEvent } = require('@testing-library/react');
require('@testing-library/jest-dom');

// Mock ItemsListingPage component for testing
function ItemsListingPage({ initialFilters }) {
  const [filters, setFilters] = React.useState(initialFilters);
  const [items] = React.useState(['Item1', 'Item2', 'Item3']);

  return React.createElement(
    'div',
    {},
    // Conditional rendering of the "Clear Filters" button if filters exist
    filters && filters.category
      ? React.createElement(
          'button',
          { role: 'button', onClick: () => setFilters(null) },
          'Clear Filters'
        )
      : null,
    // Render items list with test id "item"
    items.map((item, index) =>
      React.createElement('div', { key: index, 'data-testid': 'item' }, item)
    )
  );
}

describe('ItemsListingPage - Clear Filters Functionality', () => {
  test('Display clear filters option when filters are applied', () => {
    render(React.createElement(ItemsListingPage, { initialFilters: { category: 'Action' } }));
    
    const clearFiltersButton = screen.getByRole('button', { name: /clear filters/i });
    expect(clearFiltersButton).toBeInTheDocument();
  });

  test('Clear all filters and display full list', () => {
    const { container } = render(React.createElement(ItemsListingPage, { initialFilters: { category: 'Action' } }));
    
    const clearFiltersButton = screen.getByRole('button', { name: /clear filters/i });
    fireEvent.click(clearFiltersButton);
    
    expect(screen.queryByRole('button', { name: /clear filters/i })).not.toBeInTheDocument();
    
    const items = container.querySelectorAll('[data-testid="item"]');
    expect(items.length).toBeGreaterThan(0);
  });
});