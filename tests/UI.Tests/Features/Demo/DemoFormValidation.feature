@Category:Smoke @Feature:FormValidation @Risk:Critical @Layer:Ui @Requirement:REQ-DEMO-FORM-001
Feature: Registration Form Validation
  As a system administrator I want to ensure the registration form validates user input correctly
  So that only valid data is accepted and users receive clear feedback on errors

  Background:
    Given a Chromium browser is available

  # AC-004-01: Valid submission accepted
  @Category:Smoke @Risk:High
  Scenario: Submit form with all valid fields filled
    Given I am on the registration page
    When I enter name "Valid User"
    And I enter email "valid.user@example.com"
    And I enter password "ValidPass123!"
    And I attempt to register
    Then the form should accept the submission

  # AC-004-02: Empty name validation
  @Category:Regression @Risk:High
  Scenario: Empty name field shows validation error
    Given I am on the registration page
    When I leave the name field empty
    And I enter email "test@example.com"
    And I enter password "Pass123!"
    And I attempt to register
    Then a validation error should be displayed for the name field

  # AC-004-03: Invalid email validation
  @Category:Regression @Risk:High
  Scenario: Invalid email format shows validation error
    Given I am on the registration page
    When I enter name "Test User"
    And I enter invalid email "invalid-email"
    And I enter password "Pass123!"
    And I attempt to register
    Then I should see a validation error for email

  # AC-004-04: Empty password validation
  @Category:Regression @Risk:High
  Scenario: Empty password field shows validation error
    Given I am on the registration page
    When I enter name "Test User"
    And I enter email "test@example.com"
    And I leave the password field empty
    And I attempt to register
    Then a validation error should be displayed for the password field

  # AC-004-05: All fields empty validation
  @Category:Regression @Risk:Critical
  Scenario: All empty fields show validation errors
    Given I am on the registration page
    When I leave required fields empty
    And I attempt to register
    Then I should see multiple validation errors
    And the registration should not proceed
