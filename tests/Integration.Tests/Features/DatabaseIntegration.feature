@Category:Integration @Feature:DatabaseOperations @Risk:High @Layer:Integration @Requirement:REQ-INT-001
Feature: Database Integration Tests
  As a system component
  I want to verify database operations work correctly
  So that data is stored and retrieved reliably

  Background:
    Given the database connection is configured
    And the test database is available

  @Smoke @Risk:Critical
  Scenario: Database connection is successful
    When I attempt to connect to the database
    Then the connection should be successful
    And I should be able to execute a simple query

  @Risk:High
  Scenario: Insert and retrieve user data
    Given I have a new user record:
      | Username  | Email              | FirstName | LastName |
      | testuser1 | test1@example.com  | John      | Doe      |
    When I insert the user into the database
    Then the user should be saved successfully
    And I should be able to retrieve the user by username "testuser1"
    And the retrieved user should match the inserted data

  Scenario: Update existing user data
    Given a user exists in the database:
      | Username  | Email              | FirstName | LastName |
      | testuser2 | test2@example.com  | Jane      | Smith    |
    When I update the user email to "newemail@example.com"
    Then the user email should be updated in the database
    And the user should have email "newemail@example.com"

  Scenario: Delete user from database
    Given a user exists in the database:
      | Username  | Email              | FirstName | LastName |
      | testuser3 | test3@example.com  | Bob       | Johnson  |
    When I delete the user with username "testuser3"
    Then the user should be removed from the database
    And querying for username "testuser3" should return no results

  @Risk:Medium
  Scenario: Query users with filters
    Given multiple users exist in the database:
      | Username  | Email              | FirstName | LastName | Active |
      | user1     | user1@test.com     | Alice     | Brown    | true   |
      | user2     | user2@test.com     | Bob       | Green    | false  |
      | user3     | user3@test.com     | Charlie   | White    | true   |
    When I query for active users only
    Then I should receive 2 users
    And all returned users should have Active status true

  @Risk:High
  Scenario: Transaction rollback on error
    Given I start a database transaction
    When I insert a user with invalid data that violates constraints
    Then the transaction should be rolled back
    And no partial data should be saved to the database

  Scenario: Concurrent database operations
    Given I have 5 concurrent user creation requests
    When I execute all requests simultaneously
    Then all 5 users should be created successfully
    And there should be no data corruption
    And each user should have a unique ID

  @Risk:Critical
  Scenario: Database connection pool management
    When I create 10 simultaneous database connections
    Then all connections should be established successfully
    And connections should be returned to the pool after use
    And no connection leaks should occur

  Scenario: Execute stored procedure
    Given a stored procedure "GetUsersByRole" exists
    When I execute the stored procedure with parameter "Admin"
    Then the procedure should return users with Admin role
    And the result should contain expected columns

  Scenario Outline: Bulk insert operations
    Given I have <count> user records to insert
    When I perform a bulk insert operation
    Then all <count> records should be inserted within 5 seconds
    And I should be able to query and verify all records

    Examples:
      | count |
      | 100   |
      | 500   |
      | 1000  |

  @Risk:Medium
  Scenario: Database schema validation
    When I query the database schema
    Then the "Users" table should exist
    And the "Users" table should have required columns:
      | Column      | Type    | Nullable |
      | Id          | int     | false    |
      | Username    | varchar | false    |
      | Email       | varchar | false    |
      | FirstName   | varchar | true     |
      | LastName    | varchar | true     |
      | CreatedDate | datetime| false    |
