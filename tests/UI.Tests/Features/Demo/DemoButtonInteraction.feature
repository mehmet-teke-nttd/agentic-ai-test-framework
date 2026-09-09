@Category:Smoke @Feature:DemoButtonInteraction @Risk:Medium @Layer:Ui @Requirement:REQ-DEMO-BTN-001
Feature: DemoApps Button Interaction
  As a user I want to explore the DemoApps UI Testing features
  So that I can practice interacting with different web elements

  Background:
    Given a Chromium browser is available
    And I am on the DemoApps website

  # AC-001: Navigate to Button UI Testing page
  @Category:Smoke
  Scenario: Navigate to Button testing page from DemoApps homepage
    Given I am on the DemoApps homepage
    When I navigate to "UI Testing Concepts"
    And I select "Button" from the left menu
    Then the Button testing page should be displayed
    When I click the "Yes" button
    Then I should see the confirmation text 'You selected "Yes"'

  