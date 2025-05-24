Feature: SSC207_StreamingServicePrices

Background:
  Given I am logged in on the dashboard page

Scenario: Valid price entry for a service
  Given the "Monthly Price" input for "Hulu" is empty
  When I type "12.99" into the "Monthly Price" input for "Hulu"
  Then the value "12.99" is saved for "Hulu"
  And I see "12.99" displayed next to "Hulu"

Scenario: Monthly Price inputs are navigable via keyboard
  When I tab through the Manage Subscriptions section until the "Monthly Price" input for "Disney+" is focused
  Then focus is on the "Monthly Price" input for "Disney+"
  When I type "8.99" into that input
  And I press Enter to submit the form
  Then the new value is submitted for "Disney+"

Scenario: Monthly Price inputs and spend summary are announced by screen readers
  When focus lands on the "Monthly Price" input for "Netflix"
  Then a screen reader announces "Monthly Price input for Netflix"
  When I type "9.99" into that input
  Then the screen reader confirms "9.99"
  When I toggle "Streaming Service Analysis"
  Then the summary announces "Total Monthly Cost"