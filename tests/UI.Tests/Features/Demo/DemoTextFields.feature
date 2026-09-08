@Category:Smoke @Feature:TextFieldRegistration @Risk:High @Layer:Ui @Requirement:REQ-DEMO-TEXT-001
Feature: Text Field Registration
  As a new user I want to enter my registration details into text fields on the registration form
  So that I can create an account and proceed with the application

  Background:
    Given a Chromium browser is available

  # AC-001-01: Enter valid registration details
  @Category:Smoke
  Scenario: Enter valid registration details into text fields
    Given I am on the registration page
    When I enter name "Jane Doe"
    And I enter email "jane.doe@example.com"
    And I enter password "SecurePass123!"
    Then the entered values should be visible in the respective text fields

  # AC-001-02: Placeholder text is displayed
  @Category:Regression @Risk:Medium
  Scenario: Text fields display placeholder text
    Given I am on the registration page
    When I view the text fields
    Then each field should display its placeholder text

  # AC-001-03: Default value field contains pre-populated value
  @Category:Regression @Risk:Medium
  Scenario: Default value field contains pre-populated value
    Given I am on the registration page with a field that has a default value
    When I view the default value field
    Then the field should contain the pre-populated default value

  # AC-001-04: Captured value matches entered data
  @Category:Regression @Risk:Medium
  Scenario: Captured value matches entered data
    Given I am on the registration page
    When I enter "TestUser" into the name field
    And I read back the name field value
    Then the captured value should match "TestUser"

  # AC-001-05: Empty required fields prevent submission
  @Category:Regression @Risk:High
  Scenario: Empty required fields show validation on submit
    Given I am on the registration page
    When I leave required fields empty
    And I attempt to register
    Then validation should prevent submission or show an error
