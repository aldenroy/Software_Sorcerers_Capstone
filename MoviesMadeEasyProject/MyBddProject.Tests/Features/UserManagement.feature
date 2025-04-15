@kira
Feature: User Management

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

  Scenario: Successful Registration
    Given the user is on the registration page
    When the user enters "Test" in the first name field
    And the user enters "Testing" in the last name field
    And the user enters "test@testing.com" in the registration email field
    And the user enters "Test!123" in the registration password field
    And the user enters "Test!123" in the password confirmation field   
    And the user submits the form
    Then the user should be redirected to the preferences page

  Scenario: Unsuccessful Registration Duplicate Email
    Given the user is on the registration page
    When the user enters "Test" in the first name field
    And the user enters "Testing" in the last name field
    And the user enters "test@test.com" in the registration email field
    And the user enters "Test!123" in the registration password field
    And the user enters "Test!123" in the password confirmation field   
    And the user submits the form
    Then the user should see an error message for the duplicate email
    And the user should remain on the registration page