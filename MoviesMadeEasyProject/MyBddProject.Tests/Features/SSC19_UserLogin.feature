@kira
Feature: User Login

  Background:
    Given a user with the email "test@test.com" exists in the system

  Scenario: Successful Login
    Given the user is on the login page
    When the user enters "test@test.com" in the email field
    And the user enters "Test!123" in the password field
    Then the user will be logged in and redirected to the dashboard page

  Scenario: Unsuccessful Login
    Given the user is on the login page
    When the user enters "test@test.com" in the email field
    And the user enters "Test" in the password field
    Then the user should see an error message
    And the user should remain on the login page

