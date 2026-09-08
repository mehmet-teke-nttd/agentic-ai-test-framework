@Category:Integration @Feature:ServiceIntegration @Risk:High @Layer:Integration @Requirement:REQ-INT-002
Feature: Service-to-Service Integration Tests
  As a distributed system
  I want to verify that services communicate correctly with each other
  So that end-to-end workflows function properly

  Background:
    Given all required services are running
    And the service URLs are configured

  @Smoke @Risk:Critical
  Scenario: Authentication service integration
    Given the Authentication service is available
    When I send a login request with valid credentials:
      | Username | Password    |
      | testuser | TestPass123 |
    Then I should receive an authentication token
    And the token should be valid for at least 3600 seconds

  @Risk:High
  Scenario: Complete order processing workflow
    Given I am authenticated as user "customer1"
    And I have items in my shopping cart:
      | ProductId | Quantity | Price |
      | PROD-001  | 2        | 29.99 |
      | PROD-002  | 1        | 49.99 |
    When I submit the order through the Order service
    Then the order should be created successfully
    And I should receive an order confirmation ID
    And a notification should be sent via the Notification service
    And the order status should be "Pending"

  Scenario: Order service validates user authentication
    Given I am not authenticated
    When I attempt to create an order
    Then the Order service should reject the request
    And I should receive a 401 Unauthorized response

  @Risk:Medium
  Scenario: Notification service integration
    Given I am authenticated as user "testuser"
    When I trigger a notification event:
      | Type  | Recipient           | Subject           | Body                |
      | Email | test@example.com    | Test Notification | This is a test body |
    Then the Notification service should accept the request
    And the notification should be queued for delivery
    And I should receive a notification ID

  Scenario: Service circuit breaker functionality
    Given the Order service is configured with a circuit breaker
    And the Payment service is unavailable
    When I attempt to create 5 orders
    Then the first 3 requests should fail with service unavailable
    And the circuit should open after 3 failures
    And subsequent requests should fail fast without calling Payment service

  @Risk:High
  Scenario: Service-to-service timeout handling
    Given the Notification service has a 2-second timeout
    And the Email provider is slow to respond (3 seconds)
    When I send a notification request
    Then the request should timeout after 2 seconds
    And a timeout error should be logged
    And the notification should be marked as failed

  Scenario: Retry mechanism for transient failures
    Given the Order service is configured with retry policy
    And the Database service returns transient errors
    When I submit an order
    Then the Order service should retry 3 times
    And the order should eventually succeed
    And the retry attempts should be logged

  @Risk:Critical
  Scenario: Service dependency health checks
    When I query the health endpoint of each service
    Then all services should report healthy status:
      | Service      | Expected Status |
      | Auth         | Healthy         |
      | Order        | Healthy         |
      | Notification | Healthy         |
    And each service should report its dependencies status

  Scenario: Cross-service data consistency
    Given I create a user account in the Auth service
    When I create an order for that user
    Then the Order service should validate the user exists
    And the order should reference the correct user ID
    And querying both services should show consistent user data

  Scenario: Service authentication with JWT tokens
    Given I have a valid JWT token from Auth service
    When I call the Order service with the token
    Then the Order service should validate the token
    And the request should be authorized
    And the user identity should be extracted from the token

  @Risk:High
  Scenario Outline: Load distribution across service instances
    Given the Order service has <instances> running instances
    When I send <requests> concurrent requests
    Then the requests should be distributed across all instances
    And all requests should complete successfully
    And the response time should be under <maxTime> seconds

    Examples:
      | instances | requests | maxTime |
      | 2         | 10       | 5       |
      | 3         | 20       | 6       |

  Scenario: Service logs correlation IDs
    Given I initiate a request with correlation ID "TEST-CORR-12345"
    When I create an order that triggers multiple service calls
    Then all services should log with the same correlation ID
    And I should be able to trace the request across services

  @Risk:Medium
  Scenario: Service version compatibility
    Given Auth service is running version "2.1"
    And Order service is running version "2.0"
    When I make API calls between services
    Then the services should communicate successfully
    And backward compatibility should be maintained
