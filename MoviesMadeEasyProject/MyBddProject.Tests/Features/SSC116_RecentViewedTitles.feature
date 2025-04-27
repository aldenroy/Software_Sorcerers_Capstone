Feature: RecentViewedTitles

Scenario: Log a movie view when "View Details" is activated for "Pokemon 4Ever"
  Given I am logged in on the dashboard page
  And I have viewed "Pokemon 4Ever" movie
  Then I see a "Recently Viewed" section listing the movies, including "Pokemon 4Ever"

Scenario: Display recently viewed movies on the dashboard
  Given I am logged in on the dashboard page
  And I have viewed "Pokemon 4Ever" and then "Her"
  Then I see a "Recently Viewed" section listing the movies with the most recently viewed on the left, so "Her" appears to the left of "Pokemon 4Ever"

Scenario: Screen reader announces movies on the dashboard
  Given I am logged in on the dashboard page
  And I have viewed "Her" and then "Pokemon 4Ever"
  Then a screen reader reads the "Recently Viewed" section
  Then I hear descriptive labels for each movie that include the movie title

Scenario: Screen reader announces no viewed movies message
  Given I am logged in as a new user with no viewed movies
  Then I see the message "You haven't viewed any titles yet." announced as a status message

Scenario: Movies on the dashboard are selectable by keyboard commands
  Given I am logged in on the dashboard page
  And I have viewed "Pokemon 4Ever" movie
  When I navigate through the movies using keyboard commands (such as Tab or arrow keys)
  Then I can select and activate a movie using keyboard only, without requiring a mouse
