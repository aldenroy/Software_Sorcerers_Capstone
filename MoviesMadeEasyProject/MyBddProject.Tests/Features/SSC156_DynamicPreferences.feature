@kira
Feature: Dynamic Preferences

   Background:
      Given a user with the email "test@test.com" exists in the system

   Scenario: User updates theme to Dark Mode
      Given the user is logged in and on the Preferences page
      When the user selects "Dark" from the Theme dropdown
      Then the page should immediately switch to dark mode
      And the "team_logo_dark.png" logo should appear

   Scenario: User updates font size to large
      Given the user is logged in and on the Preferences page
      When the user selects "Large" from the font size dropdown
      Then the font size should immediately switch to large

   Scenario: User updates font type to open dyslexic
      Given the user is logged in and on the Preferences page
      When the user selects "Open Dyslexic" from the font type dropdown
      Then the font type should immediately switch to open dyslexic