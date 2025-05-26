Feature: Filter and sort items by rating range

  Background:
    Given I am on the "Movie Search" page
    And a list of items with ratings out of 100 is displayed
    And the rating-range inputs is visible

  Scenario: Filter items within a valid rating range
    Given the minimum rating is set to 30
    And the maximum rating is set to 85
    Then I should see only items with ratings ≥ 30 and ≤ 85

  Scenario: Show validation error for an invalid rating range
    Given the minimum rating is set to 90
    And the maximum rating is set to 50
    Then I should see an inline validation error "The rating range is invalid: Min Rating must be ≤ Max Rating."

  Scenario: Show message when no items match the range
    Given the minimum rating is set to 95
    And the maximum rating is set to 100
    And no items have ratings between 95 and 100
    Then I should see "No movies found for that rating range." in the results container
