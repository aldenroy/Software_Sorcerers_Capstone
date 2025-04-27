Feature: Content Filtering by Streaming Service
  In order to view only desired content
  As a user on the content browsing page
  I want to filter content by streaming services

  Scenario: Filtering content by a single streaming service
    Given the user is on the content browsing page
    And the list of available streaming services is displayed
    When the user selects a specific streaming service "Netflix"
    Then the content list is updated to show only items available on "Netflix"

#  Scenario: Filtering content by multiple streaming services
#    Given the user is on the content browsing page
#    And the list of available streaming services is displayed
#    When the user selects streaming services "Netflix" and "Hulu"
#    Then the content list is updated to show items available on either "Netflix" or "Hulu"
#
#  Scenario: Clearing applied streaming service filters
#    Given the user has one or more streaming service filters applied
#    When the user clicks the "Clear Filters" button
#    Then all streaming service filters are removed
#    And the content list returns to the default unfiltered view
#
#  Scenario: Filter response time performance
#    Given the user is on the content browsing page
#    And the user has one or more streaming service filters applied
#    When the user selects a streaming service filter
#    Then the content list should update within 2 seconds