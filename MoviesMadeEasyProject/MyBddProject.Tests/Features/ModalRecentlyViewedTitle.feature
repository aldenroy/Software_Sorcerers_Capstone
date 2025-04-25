Feature: Recently-viewed show modal

  Background:
    Given I am logged in on the dashboard page
      And I have viewed "Pokemon 4Ever" movie
      And I see a "Recently Viewed" section listing the movies, including "Pokemon 4Ever"

  Scenario: Clicking a recently viewed show opens its modal
    When I click the show "Pokemon 4Ever" in the recently viewed section
    Then a show-details modal is displayed for "Pokemon 4Ever"

  Scenario: Modal can be closed
    Given the show-details modal is displayed for "Pokemon 4Ever"
     When I click the modal close button
     Then the modal is no longer visible

  Scenario: Modal is accessible via keyboard navigation
     When I tab to "Pokemon 4Ever" in the recently viewed section
      And I press Enter
     Then a show-details modal is displayed for "Pokemon 4Ever"
      And focus moves inside the modal

