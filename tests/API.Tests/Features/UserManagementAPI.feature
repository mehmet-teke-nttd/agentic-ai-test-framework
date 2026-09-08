@Category:Smoke @Feature:UserAPI @Risk:High @Layer:Api @Requirement:REQ-API-001
Feature: User Management API
  As a client application
  I want to interact with the User Management API
  So that I can create, read, update, and delete user records

  Background:
    Given the API is available at the configured base URL

  @Smoke
  Scenario: Get all users successfully
    When I send a GET request to "/users"
    Then the response status code should be 200
    And the response should contain a list of users
    And each user should have required fields "id, name, email"

  @Smoke
  Scenario: Get a single user by ID
    When I send a GET request to "/users/1"
    Then the response status code should be 200
    And the response should contain a user with id "1"
    And the user should have a valid email address

  @Risk:Critical
  Scenario: Create a new user
    Given I have the following user data:
      | name  | username | email              |
      | John Doe | johndoe  | john@example.com   |
    When I send a POST request to "/users" with the user data
    Then the response status code should be 201
    And the response should contain the created user
    And the created user should have an "id" field

  Scenario: Update an existing user
    Given a user exists with id "1"
    And I have updated user data:
      | name        |
      | Jane Smith  |
    When I send a PUT request to "/users/1" with the updated data
    Then the response status code should be 200
    And the response should contain the updated user
    And the user "name" should be "Jane Smith"

  Scenario: Partially update a user with PATCH
    Given a user exists with id "1"
    When I send a PATCH request to "/users/1" with:
      | email               |
      | newemail@test.com   |
    Then the response status code should be 200
    And the user "email" should be "newemail@test.com"

  Scenario: Delete a user
    Given a user exists with id "1"
    When I send a DELETE request to "/users/1"
    Then the response status code should be 200

  @Risk:Medium
  Scenario: Get user with non-existent ID returns empty
    When I send a GET request to "/users/99999"
    Then the response status code should be 404

  Scenario: Create user with invalid email
    Given I have the following user data:
      | name      | username | email        |
      | Bad User  | baduser  | invalid-email |
    When I send a POST request to "/users" with the user data
    Then the response status code should be 400 or 422

  @Risk:High
  Scenario: API response time is acceptable
    When I send a GET request to "/users"
    Then the response should be received within 2000 milliseconds
    And the response status code should be 200

  Scenario: API returns proper content type
    When I send a GET request to "/users/1"
    Then the response status code should be 200
    And the response content type should be "application/json"

  Scenario Outline: Get users by query parameters
    When I send a GET request to "/users" with query parameter "<parameter>" equals "<value>"
    Then the response status code should be 200
    And the response should be filtered by "<parameter>"

    Examples:
      | parameter | value          |
      | email     | Rey.Padberg@   |
      | username  | Bret           |
