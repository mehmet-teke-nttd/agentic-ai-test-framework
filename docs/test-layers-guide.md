# Test Layers Guide

This framework implements a complete test pyramid with three distinct layers:

## Test Pyramid Overview

```
         /\
        /UI\          Fewer, Slower, More Expensive
       /----\         - Browser-based end-to-end tests
      / API  \        - REST API endpoint tests
     /--------\       - Database queries, service calls
    /Integration\     
   /____________\     More, Faster, Less Expensive
```

## Layer 1: UI Tests (`tests/UI.Tests`)

### Purpose
Test the application through the browser, validating the complete user experience including:
- Visual elements and layouts
- User interactions (clicks, form submissions)
- Navigation flows
- Client-side JavaScript behavior

### Technology Stack
- **ReqnRoll**: BDD framework for Gherkin scenarios
- **Playwright**: Browser automation
- **NUnit**: Test runner

### Configuration

`appsettings.json`:
```json
{
  "Application": {
    "BaseUrl": "https://demoapps.qspiders.com/"
  }
}
```

Environment variable override:
```powershell
$env:TEST_APP_BASE_URL = "https://qa.example.com"
```

### Feature File Example

```gherkin
@Category:Smoke @Feature:Checkout @Risk:High @Layer:Ui
Feature: User Registration
  
  Scenario: Successful registration
    Given I am on the registration page
    When I enter valid user details
    And I click "Register"
    Then I should see the confirmation message
```

### When to Use UI Tests
- ✅ Critical user journeys (checkout, registration, login)
- ✅ Visual regression testing
- ✅ Cross-browser compatibility
- ❌ Data validation logic (use API tests instead)
- ❌ Business rules (use Integration tests instead)

### Running UI Tests

```powershell
# All UI tests
dotnet test tests/UI.Tests/UI.Tests.csproj

# Smoke tests only
dotnet test tests/UI.Tests --filter "TestCategory=Smoke"

# Through the gateway
{"schemaVersion":"1.0","tool":"run_tests","arguments":{"projectId":"ui-tests"}}
```

---

## Layer 2: API Tests (`tests/API.Tests`)

### Purpose
Test REST API endpoints directly, bypassing the UI:
- Request/response validation
- HTTP status codes
- JSON schema validation
- API performance
- Authentication and authorization

### Technology Stack
- **ReqnRoll**: BDD framework
- **HttpClient**: .NET HTTP client
- **Newtonsoft.Json**: JSON serialization
- **NUnit**: Test runner

### Configuration

`appsettings.json`:
```json
{
  "Api": {
    "BaseUrl": "https://jsonplaceholder.typicode.com",
    "Timeout": 30
  }
}
```

Environment variable override:
```powershell
$env:TEST_API_BASE_URL = "https://api.qa.example.com"
```

### Feature File Example

```gherkin
@Category:Smoke @Feature:UserAPI @Risk:High @Layer:Api
Feature: User Management API
  
  Scenario: Create a new user
    Given I have the following user data:
      | name     | email            |
      | John Doe | john@example.com |
    When I send a POST request to "/users" with the user data
    Then the response status code should be 201
    And the response should contain the created user
```

### When to Use API Tests
- ✅ CRUD operations validation
- ✅ API contract testing
- ✅ Performance testing (response times)
- ✅ Error handling and validation
- ✅ Data-driven scenarios
- ✅ Faster feedback than UI tests

### Running API Tests

```powershell
# All API tests
dotnet test tests/API.Tests/API.Tests.csproj

# High-risk tests only
dotnet test tests/API.Tests --filter "FullyQualifiedName~Risk:High"

# Through the gateway
{"schemaVersion":"1.0","tool":"run_tests","arguments":{"projectId":"api-tests","categories":["Smoke"]}}
```

### Available Features
- `UserManagementAPI.feature`: User CRUD operations
- `PostsManagementAPI.feature`: Blog posts and comments

---

## Layer 3: Integration Tests (`tests/Integration.Tests`)

### Purpose
Test how system components work together:
- Database operations (CRUD, transactions, concurrency)
- Service-to-service communication
- Message queues
- External dependencies
- Cross-cutting concerns (auth, logging, correlation)

### Technology Stack
- **ReqnRoll**: BDD framework
- **HttpClient**: Service communication
- **Microsoft.Data.SqlClient**: Database access
- **NUnit**: Test runner

### Configuration

`appsettings.json`:
```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=TestDB;Trusted_Connection=True;",
    "CommandTimeout": 30
  },
  "Services": {
    "AuthServiceUrl": "http://localhost:5001",
    "OrderServiceUrl": "http://localhost:5002",
    "NotificationServiceUrl": "http://localhost:5003"
  }
}
```

Environment variable overrides:
```powershell
$env:TEST_DB_CONNECTION_STRING = "Server=..."
```

### Feature File Example

```gherkin
@Category:Integration @Feature:DatabaseOperations @Risk:High @Layer:Integration
Feature: Database Integration Tests
  
  Scenario: Insert and retrieve user data
    Given I have a new user record:
      | Username  | Email             |
      | testuser1 | test1@example.com |
    When I insert the user into the database
    Then the user should be saved successfully
    And I should be able to retrieve the user by username "testuser1"
```

### When to Use Integration Tests
- ✅ Database schema validation
- ✅ Transaction rollback behavior
- ✅ Concurrent operations
- ✅ Service authentication flows
- ✅ Circuit breakers and retries
- ✅ Message queue integration
- ✅ Cache invalidation

### Running Integration Tests

```powershell
# All integration tests
dotnet test tests/Integration.Tests/Integration.Tests.csproj

# Database tests only
dotnet test tests/Integration.Tests --filter "Feature=DatabaseOperations"

# Through the gateway
{"schemaVersion":"1.0","tool":"run_tests","arguments":{"projectId":"integration-tests"}}
```

### Available Features
- `DatabaseIntegration.feature`: Database CRUD, transactions, concurrency
- `ServiceIntegration.feature`: Service-to-service communication, auth, resilience

---

## Test Metadata Tags

All layers use consistent tagging:

### @Category
Groups related tests:
- `@Category:Smoke` - Critical path tests
- `@Category:Regression` - Full test suite
- `@Category:Integration` - Integration-specific

### @Feature
Business feature being tested:
- `@Feature:UserAPI`
- `@Feature:Checkout`
- `@Feature:DatabaseOperations`

### @Risk
Impact of failure:
- `@Risk:Critical` - Production-blocking issues
- `@Risk:High` - Major functionality broken
- `@Risk:Medium` - Degraded experience
- `@Risk:Low` - Minor issues

### @Layer
Test pyramid layer:
- `@Layer:Ui` - UI tests
- `@Layer:Api` - API tests
- `@Layer:Integration` - Integration tests

### @Requirement
Traceability to requirements:
- `@Requirement:REQ-API-001`

---

## Running Tests Across All Layers

### By Risk Level
```powershell
# All critical tests across all layers
dotnet test --filter "FullyQualifiedName~Risk:Critical"
```

### By Category
```powershell
# All smoke tests
dotnet test --filter "TestCategory=Smoke"
```

### By Layer
```powershell
# Only API layer
dotnet test --filter "TestCategory=Api"

# Only Integration layer
dotnet test --filter "TestCategory=Integration"
```

### Through the Gateway
```powershell
# Start the gateway
dotnet run --project agent/Test.AgentGateway.Cli

# Discover all tests
{"schemaVersion":"1.0","id":"1","tool":"discover_tests","arguments":{}}

# Run smoke tests from API layer
{"schemaVersion":"1.0","id":"2","tool":"run_tests","arguments":{"projectId":"api-tests","categories":["Smoke"]}}

# Run critical integration tests
{"schemaVersion":"1.0","id":"3","tool":"run_tests","arguments":{"projectId":"integration-tests","categories":["Critical"]}}
```

---

## Best Practices

### Test Distribution (Ideal Pyramid)
- **70%** Integration & API tests (fast, reliable)
- **20%** API-only tests (medium speed)
- **10%** UI tests (slow, expensive)

### Speed Comparison
- **Integration Tests**: ~100-500ms per test
- **API Tests**: ~50-200ms per test
- **UI Tests**: ~5-30 seconds per test

### When to Add Tests at Each Layer

#### Add UI Test When:
- Testing visual elements or layout
- Testing browser-specific behavior
- Testing complete user workflows
- Testing JavaScript interactions

#### Add API Test When:
- Testing API contracts and responses
- Testing data validation logic
- Testing API-level error handling
- Need faster feedback than UI tests

#### Add Integration Test When:
- Testing database operations
- Testing service communication
- Testing transaction behavior
- Testing system resilience (retries, timeouts)

### Avoid Duplication
- Don't test the same logic at multiple layers
- Test business logic at API/Integration layer
- Reserve UI tests for user experience validation
- Use API tests to set up data for UI tests

---

## Debugging Failed Tests

### Check Evidence Files
Each test run creates evidence in `Evidence/<runId>/`:
- `test-result.json` - Normalized results
- `stdout.log` / `stderr.log` - Test output
- `results.trx` - Original TRX file
- `manifest.json` - All evidence files
- Layer-specific evidence:
  - UI: Screenshots, Playwright traces
  - API: Request/response logs (if configured)
  - Integration: Database snapshots (if configured)

### Analyze Failures
```powershell
# Get test results
{"tool":"get_test_result","arguments":{"runId":"<runId>"}}

# Get evidence list
{"tool":"get_test_evidence","arguments":{"runId":"<runId>"}}

# Analyze specific failure
{"tool":"analyze_test_failure","arguments":{"runId":"<runId>","testId":"<testId>"}}
```

### Common Failure Classifications
- **ProductDefect**: Assertion failures, expected vs actual mismatches
- **TestDefect**: Test code issues, selector problems, flaky tests
- **Environment**: Missing services, connectivity issues, browser not installed
- **Infrastructure**: Timeout, out of memory, testhost crashes

---

## Adding New Tests

### 1. Create Feature File
Place in appropriate `Features/` directory with proper metadata tags.

### 2. Generate Step Definitions
ReqnRoll will generate step definition stubs on first build.

### 3. Implement Steps
Add logic in the `Steps/` directory.

### 4. Run Locally
```powershell
dotnet test <project> --filter "FullyQualifiedName~<scenario-name>"
```

### 5. Verify Through Gateway
```powershell
dotnet run --project agent/Test.AgentGateway.Cli
{"tool":"run_tests","arguments":{"projectId":"<project-id>","testIds":["<test-id>"]}}
```

---

## Next Steps

1. **Customize API endpoints**: Update `API.Tests/appsettings.json` to point to your API
2. **Configure database**: Set up test database and update `Integration.Tests/appsettings.json`
3. **Add your tests**: Create feature files based on your application's requirements
4. **Integrate with CI/CD**: Run tests in your pipeline using the gateway
5. **Monitor trends**: Track test results over time using the evidence storage
