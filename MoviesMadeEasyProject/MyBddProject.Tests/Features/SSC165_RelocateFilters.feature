Feature: Off-Canvas Movie Filters
  In order to find movies that match my criteria
  As a user
  I want to use an off-canvas filter panel to refine the content list

  Scenario: Off-canvas filter panel on desktop
    Given the user is on the content browsing page using a desktop browser
    When the page loads
    Then the filter toggle button is visible in the header
    And the filter panel is hidden off-canvas
    When the user clicks the filter toggle button
    Then the filter panel slides in from the left as an off-canvas panel
    And the main content is dimmed behind the panel

  Scenario: Filter functionality remains operational
    Given the user is on the content browsing page using a desktop browser
    And the filter panel is hidden off-canvas
    When the user clicks the filter toggle button
    And the user selects one or more filter options
    Then the content list updates accordingly based on the selected filters

  Scenario: Filter movies by minimum and maximum release year
    Given the user is on the content browsing page using a desktop browser
    And the filter panel is hidden off-canvas
    When the user clicks the filter toggle button
    And the user sets the "Min Year" slider to 1990
    And the user sets the "Max Year" slider to 2000
    And the user clicks the "Apply" button
    Then only movies released between 1990 and 2000 are displayed in the content list

  Scenario: Clear Filters functionality is available and functional
    Given the user is on the content browsing page using a desktop browser
    And the user has one or more filters applied
    When the user clicks the "Clear Filters" button in off-canvas panel
    Then all applied filters are removed and the content list returns to the default view

  Scenario: Consistent filter layout across supported browsers
    Given the user accesses the content browsing page on a supported browser
    When the page loads
    Then the filter toggle button is visible in the header
    And the filter panel is hidden off-canvas
