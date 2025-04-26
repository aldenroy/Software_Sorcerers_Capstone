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

  Scenario: Navigation results
    Given the user is in the search section 
    When the user enters the movie "Hunger Games" in the search bar
    Then the search should show the results for "Hunger Games"

  Scenario: Clicking More Like This button
    Given the user has searched for the "Hunger Games"
    When the user clicks the "More Like This" button for the first result
    Then the user should be redirected to a page with the Openai results



#   Scenario: Navigate to the recommendations details modal
#     Given the user is on the recommendations page
#     When the user clicks the "View Details" button on the first result
#     Then the user should see the details modal pop up

#   Scenario: Check if recommendations streaming service icons exist
#     Given the user has clicked the View Details button from recommendations
#     When the user is on the modal pop up
#     Then the user should see the streaming service icons

#   Scenario: Check that redirection to recommendations service works
#     Given the user is on the modal pop up
#     When the user clicks the first service icon
#     Then the user should be redirected to the respective login page
