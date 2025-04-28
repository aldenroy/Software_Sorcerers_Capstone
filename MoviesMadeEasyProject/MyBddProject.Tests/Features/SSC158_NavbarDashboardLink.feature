Feature: Dashboard UI for authenticated users

Background:
	Given I am logged in on the dashboard page

Scenario: Display Dashboard link for Authenticated User
    When the page loads
    Then I should see a "Dashboard" link in the navbar

Scenario: Navigate to the Dashboard Page
    Given I navigate to the "Home" page
    When I click the "Dashboard" link on the navbar
    Then I should be redirected to my dashboard page

Scenario: Keyboard Navigation for Dashboard Button
    Given I am logged in on the dashboard page
    When I tab through the navbar until I reach the "Dashboard" link
    Then I should be able to focus on and activate the button using the keyboard

Scenario: Screen Reader Accessibility for Dashboard link
    When I navigate to the navbar
    Then the "Dashboard" link should include a clear, descriptive label that lets my screen reader announce its purpose.



