@chatbox
Feature: Testing chatbox
  Scenario: Open chatbox
    Given the user is on the main page
    When the user clicks the up chevron
    Then the chatbox should pop out

  Scenario: Checking welcome
    Given the user has opened the chatbox
    When the user sees the chatbox
    Then the user should see a welcome message

  Scenario: Sending message
    Given the user is in the chatbox
    When the user types a message into the textbox
    Then the user should see a reply message

  Scenario: Close chatbox
    Given the user is on the open chatbox
    When the user clicks the down chevron
    Then the chatbox should pop down
