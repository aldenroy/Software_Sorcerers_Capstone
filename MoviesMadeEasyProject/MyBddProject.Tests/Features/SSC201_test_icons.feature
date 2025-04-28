@service_links
Feature: Show streaming service icons and links in view details
  Scenario: Search result
    Given the user is on the search page for icons test
    When the user enters "Hunger Games" into the search bar
    Then the search should show results for "Hunger Games"

  Scenario: Triggering View Details button
    Given the user has searched for the "Hunger Games" movie
    When the user clicks View Details on the first result
    Then the user should see the view details modal pop up

  Scenario: Check if streaming service icons exist
    Given the user has clicked the View Details button from search results
    When the user is on the modal pop up
    Then the user should see the Apple TV, Netflix, and Prime Video icons

  Scenario: Check that redirection to service works
    Given the user is on the modal pop up
    When the user clicks the netflix icon
    Then the user should be redirected to the Netflix login page

  Scenario: Opening recommendation details
    Given the user is on the recommendations page with Openai results
    When the user clicks the "View Details" button for the first result
    Then the user should see service icons on the modal

  Scenario: Checking recommendation icons
    Given the user is on the recommendations first results modal
    When the user clicks the first service icon
    Then the user should be redirected to that login page

