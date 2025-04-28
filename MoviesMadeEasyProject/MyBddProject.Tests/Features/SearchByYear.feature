@YearFilter
Feature: Filtering items by year
  In order to find items from a particular year
  As a user
  I want the list to update automatically when I select or enter a year

Scenario: Filtering items by a specific year
  Given the items listing page displays items from various years
When I set the min year slider to "2001" and the max year slider to "2008"
Then only items between year "2001" and "2008" are displayed

