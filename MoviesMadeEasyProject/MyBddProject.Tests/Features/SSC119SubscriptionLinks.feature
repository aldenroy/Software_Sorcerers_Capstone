Feature: SSC126SubscriptionLinks

Background:
	Given I am logged in on the dashboard page

Scenario: Icon Navigation to Subscription Login
    When I click on a subscription bubble for "Hulu"
    Then I should be redirected to that services website login page "https://auth.hulu.com/web/login"

Scenario: Keyboard Navigation for Subscription Icons
    When I tab through the subscription icons until I reach the "Hulu" icon
    And I focus on and activate the subscription icon using the keyboard
    Then I should be redirected to that services website login page "https://auth.hulu.com/web/login"

Scenario: Screen Reader Accessibility for Subscription Icons
    When I navigate to the subscription icons
    Then the "Hulu" subscription icon should include a clear, descriptive accessible label