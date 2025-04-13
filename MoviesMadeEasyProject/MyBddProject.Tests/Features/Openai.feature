Feature: Openai
  Scenario: Search result
    Given the user is on the search page
    When the user enters "Hunger Games" in the search bar
    Then the user search should show results for "Hunger Games"

  Scenario: Triggering More Like This button
    Given the user has searched for "Hunger Games"
    When the user clicks the "More Like This" button on the first result
    Then the user should be redirected to a new page with the Openai results

  Scenario: Check if results exist
    Given the user has clicked the More Like This button
    When the user is on the recommendations page
    Then the user should see five suggested results

  Scenario: Navigate back to search page
  Given the user is on the recommendations page
  When the user clicks the Back to search button
  Then the user should be redirected back to the search page