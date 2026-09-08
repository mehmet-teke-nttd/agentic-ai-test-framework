@Category:Smoke @Feature:CheckboxPreferences @Risk:Medium @Layer:Ui @Requirement:REQ-DEMO-CHK-001
Feature: Checkbox Notification Preferences
  As a customer completing an order I want to select my notification and product preference checkboxes
  So that I receive updates on my preferred platforms and product recommendations

  Background:
    Given a Chromium browser is available

  # AC-003-01: Select Email notification
  @Category:Smoke
  Scenario: Select Email notification checkbox
    Given I am on the demo checkbox page
    When I check the demo "Email" notification checkbox
    Then the demo "Email" checkbox should be checked

  # AC-003-02: Select WhatsApp and Message notifications
  @Category:Regression
  Scenario: Select WhatsApp and Message notification checkboxes
    Given I am on the demo checkbox page
    When I check the demo "WhatsApp" notification checkbox
    And I check the demo "Message" notification checkbox
    Then the demo "WhatsApp" checkbox should be checked
    And the demo "Message" checkbox should be checked

  # AC-003-03: Select Shoes product recommendation
  @Category:Regression
  Scenario: Select Shoes product recommendation checkbox
    Given I am on the demo checkbox page
    When I check the demo "Shoes" recommendation checkbox
    Then the demo "Shoes" checkbox should be checked

  # AC-003-04: Select all checkboxes
  @Category:Regression
  Scenario: Select all notification and recommendation checkboxes
    Given I am on the demo checkbox page
    When I select all demo notification and recommendation checkboxes
    Then all demo checkboxes should be checked

  # AC-003-05: Continue with all checkboxes selected
  @Category:Smoke
  Scenario: Continue after selecting all checkboxes
    Given I am on the demo checkbox page
    And I have selected all demo notification and recommendation checkboxes
    When I click the demo Continue button
    Then the demo checkbox page should proceed without errors

  # AC-003-06: Default unchecked state
  @Category:Regression @Risk:Low
  Scenario: All checkboxes are unchecked by default
    Given I am on the demo checkbox page
    When no demo checkboxes have been selected
    Then all demo checkboxes should be unchecked by default
