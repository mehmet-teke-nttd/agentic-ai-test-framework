@Category:Smoke @Feature:DropdownProductSelection @Risk:High @Layer:Ui @Requirement:REQ-DEMO-DROP-001
Feature: Multi-Select Dropdown Product Selection
  As a shopper I want to select products from multi-select and cascading dropdown menus
  So that I can configure my order with the correct product and location options

  Background:
    Given a Chromium browser is available

  # AC-002-01: Select single product
  @Category:Smoke
  Scenario: Select a single product from the product dropdown
    Given I am on the multi-select dropdown page
    When I select a single product from the product dropdown
    Then the selected product should appear in the chosen products list

  # AC-002-02: Select multiple products
  @Category:Regression
  Scenario: Select multiple products from the product dropdown
    Given I am on the multi-select dropdown page
    When I select multiple products from the product dropdown
    Then all selected products should appear in the chosen products list

  # AC-002-03, AC-002-04, AC-002-05: Cascading dropdown selection
  @Category:Regression @Risk:High
  Scenario: Select country state and city in cascading dropdowns
    Given I am on the multi-select dropdown page
    When I select a country using visible text
    And I select a state using value attribute
    And I select a city using index
    Then all dropdowns should reflect the selected values

  # AC-002-06: Continue with valid selections
  @Category:Smoke
  Scenario: Continue after valid dropdown selections
    Given I am on the multi-select dropdown page
    And I have made valid dropdown selections
    When I click the Continue button on the dropdown page
    Then the dropdown page should proceed without errors
