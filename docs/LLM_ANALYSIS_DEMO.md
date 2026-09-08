# LLM Failure Analysis Demo

This document demonstrates the LLM-powered failure analysis in action with real examples.

## Setup

1. **Set API Key**
```bash
export LLM_API_KEY="sk-..."  # Your OpenAI API key
```

2. **Enable LLM Analyzer**
Edit `agent/Test.AgentGateway.Cli/appsettings.json`:
```json
{
  "LlmAnalyzer": {
    "Enabled": true,
    "Provider": "OpenAI",
    "Model": "gpt-4o-mini"
  }
}
```

3. **Start Gateway**
```bash
cd agent/Test.AgentGateway.Cli
dotnet run
```

---

## Example 1: UI Test Selector Failure

### Failure Scenario
```gherkin
Scenario: User clicks submit button
  Given I am on the registration page
  When I click the submit button
  Then I should see confirmation message
```

### Error Message
```
Error: Strict mode violation: locator('button[type="submit"]') resolved to 3 elements
  at RegistrationPage.clickSubmit (RegistrationPage.cs:45)
  at RegistrationSteps.WhenIClickSubmitButton (Steps.cs:78)
```

### Input Request
```json
{
  "schemaVersion": "1.0",
  "id": "demo-1",
  "tool": "analyze_test_failure",
  "arguments": {
    "runId": "20260904T150055123Z-demo1",
    "testId": "UserRegistration.ClickSubmitButton"
  }
}
```

### LLM Analysis Output
```json
{
  "schemaVersion": "1.0",
  "id": "demo-1",
  "success": true,
  "result": {
    "runId": "20260904T150055123Z-demo1",
    "testId": "UserRegistration.ClickSubmitButton",
    "classification": "TestDefect",
    "confidence": 0.92,
    "reasons": [
      "Playwright strict mode detected multiple elements matching selector 'button[type=\"submit\"]'",
      "Page contains 3 submit buttons: main form submit, newsletter signup, and quick action button",
      "Selector is not specific enough to uniquely identify the intended button",
      "This is a common test code issue where generic selectors match unintended elements",
      "[Recommendation] Use page.getByRole('button', { name: 'Register' }) for role-based selection",
      "[Recommendation] Add data-testid='register-submit-btn' to the target button in the app",
      "[Recommendation] Use page.locator('form.registration-form button[type=\"submit\"]') to scope to registration form",
      "[Recommendation] Ensure tests wait for page to fully load before clicking"
    ],
    "citedEvidence": [
      "screenshot_failure.png",
      "playwright-trace.zip",
      "test-results.trx",
      "stdout.log"
    ]
  }
}
```

### Comparison with Deterministic
```json
{
  "tool": "compare_analyzers",
  "arguments": {
    "runId": "20260904T150055123Z-demo1",
    "testId": "UserRegistration.ClickSubmitButton"
  }
}
```

**Response:**
```json
{
  "deterministicAnalysis": {
    "classification": "TestDefect",
    "confidence": 0.70,
    "reasons": [
      "Failure matches a test-code or locator signature."
    ]
  },
  "llmAnalysis": {
    "classification": "TestDefect",
    "confidence": 0.92,
    "reasons": [
      "Playwright strict mode detected multiple elements...",
      "[Recommendation] Use page.getByRole('button', { name: 'Register' })...",
      "..."
    ]
  },
  "comparison": {
    "classificationMatches": true,
    "confidenceDifference": 0.22,
    "recommendation": "Both agree on classification, but LLM analyzer has significantly higher confidence with specific actionable recommendations."
  }
}
```

---

## Example 2: API Test - Unexpected Response

### Failure Scenario
```gherkin
Scenario: Get user by ID
  When I send a GET request to "/users/123"
  Then the response status code should be 200
  And the user email should be valid
```

### Error Message
```
AssertionException: Expected email to contain '@' but got 'john.doe.example.com'
```

### LLM Analysis
```json
{
  "classification": "ProductDefect",
  "confidence": 0.88,
  "reasons": [
    "API returned malformed email address missing '@' symbol",
    "Email validation on backend appears broken or missing",
    "This is a data integrity issue in the application, not test code",
    "Similar pattern could affect other user records",
    "[Recommendation] File bug report for email validation in user creation endpoint",
    "[Recommendation] Add backend validation to ensure emails contain '@' before saving",
    "[Recommendation] Review existing user records for similar data corruption",
    "[Recommendation] Add API contract tests to prevent regression"
  ],
  "citedEvidence": [
    "test-results.trx",
    "stdout.log"
  ]
}
```

---

## Example 3: Integration Test - Database Timeout

### Failure Scenario
```gherkin
Scenario: Bulk insert users
  Given I have 1000 user records
  When I perform bulk insert
  Then all records should be inserted within 5 seconds
```

### Error Message
```
Timeout: The operation has timed out. 
  at Microsoft.Data.SqlClient.SqlCommand.ExecuteNonQuery()
  at DatabaseSteps.InsertBulkUsers (DatabaseSteps.cs:145)
```

### LLM Analysis
```json
{
  "classification": "Infrastructure",
  "confidence": 0.82,
  "reasons": [
    "Database command timeout indicates resource or performance issue",
    "Bulk insert of 1000 records should complete quickly on healthy database",
    "Possible causes: slow connection, database under load, missing indexes, or lock contention",
    "This is not a test code issue - the test logic is correct",
    "[Recommendation] Check database server CPU and memory usage during test execution",
    "[Recommendation] Verify database connection pooling is configured correctly",
    "[Recommendation] Add indexes on frequently queried columns (Username, Email)",
    "[Recommendation] Consider increasing CommandTimeout or using batched inserts",
    "[Recommendation] Review concurrent test execution - may cause database contention"
  ],
  "citedEvidence": [
    "test-results.trx",
    "stderr.log"
  ]
}
```

---

## Example 4: Flaky Test Detection

### Historical Pattern Analysis
```json
{
  "tool": "analyze_failure_patterns",
  "arguments": {
    "testId": "LoginTest.SuccessfulLogin"
  }
}
```

**Response:**
```json
{
  "testId": "LoginTest.SuccessfulLogin",
  "totalFailures": 18,
  "recentFailures": 9,
  "isFlaky": true,
  "commonClassification": "TestDefect",
  "classificationDistribution": {
    "TestDefect": 7,
    "Environment": 2
  },
  "averageConfidence": 0.65,
  "insights": [
    "Test has failed 9 times in the last 30 days.",
    "⚠️ Test appears to be FLAKY - failure classification varies across runs.",
    "Recommendation: Investigate test stability and add proper waits/retries.",
    "⚠️ Low average confidence (65%) - failures may be complex or novel.",
    "📉 Trend: Analysis confidence declining - test may be degrading."
  ]
}
```

### Detailed History
```json
{
  "tool": "get_failure_history",
  "arguments": {
    "testId": "LoginTest.SuccessfulLogin"
  }
}
```

**Recent Entries:**
```json
[
  {
    "testId": "LoginTest.SuccessfulLogin",
    "runId": "20260904T120000000Z-abc",
    "timestamp": "2026-09-04T12:00:00Z",
    "classification": "TestDefect",
    "confidence": 0.75,
    "reasons": ["Element not found: input[name='username']"],
    "duration": "00:00:03.245"
  },
  {
    "testId": "LoginTest.SuccessfulLogin",
    "runId": "20260903T140000000Z-def",
    "timestamp": "2026-09-03T14:00:00Z",
    "classification": "Environment",
    "confidence": 0.60,
    "reasons": ["Network timeout connecting to authentication service"],
    "duration": "00:00:30.000"
  }
]
```

---

## Example 5: Complete Workflow

### Step-by-Step Demo

#### 1. Run Test
```bash
dotnet run --project agent/Test.AgentGateway.Cli
```

```json
{"tool":"run_tests","arguments":{"projectId":"ui-tests","testIds":["UserRegistration.InvalidEmail"]}}
```

**Response:**
```json
{
  "success": true,
  "result": {
    "runId": "20260904T151234567Z-xyz",
    "outcome": "Failed",
    "tests": [
      {
        "testId": "UserRegistration.InvalidEmail",
        "outcome": "Failed",
        "duration": "00:00:05.123",
        "failure": {
          "message": "Expected validation error message but registration succeeded",
          "stackTrace": "at RegistrationSteps.ThenIShouldSeeValidationError..."
        }
      }
    ]
  }
}
```

#### 2. Analyze with LLM
```json
{"tool":"analyze_test_failure","arguments":{"runId":"20260904T151234567Z-xyz","testId":"UserRegistration.InvalidEmail"}}
```

**Response:**
```json
{
  "classification": "ProductDefect",
  "confidence": 0.94,
  "reasons": [
    "Application accepted invalid email format 'notanemail' without validation",
    "Expected behavior: reject invalid emails with error message",
    "Actual behavior: registration succeeded with malformed email",
    "This indicates missing or broken email validation in the backend",
    "[Recommendation] Add email format validation in user registration endpoint",
    "[Recommendation] Implement regex pattern: ^[\\w.-]+@[\\w.-]+\\.\\w+$",
    "[Recommendation] Return 400 Bad Request with error message for invalid emails",
    "[Recommendation] Add unit tests for email validation logic"
  ]
}
```

#### 3. Compare Analyzers
```json
{"tool":"compare_analyzers","arguments":{"runId":"20260904T151234567Z-xyz","testId":"UserRegistration.InvalidEmail"}}
```

**Result:** Both agree it's a ProductDefect, LLM provides detailed fix steps

#### 4. Check Historical Pattern
```json
{"tool":"analyze_failure_patterns","arguments":{"testId":"UserRegistration.InvalidEmail"}}
```

**Insight:** First failure - no pattern yet, but will track going forward

#### 5. Apply Fix & Verify
- Developer adds email validation
- Re-run test → Passes
- Historical tracker records resolution

---

## Cost Analysis

### Token Usage per Example

| Example | Input Tokens | Output Tokens | Cost (gpt-4o-mini) |
|---------|--------------|---------------|-------------------|
| Example 1 (UI) | ~800 | ~300 | $0.00033 |
| Example 2 (API) | ~600 | ~250 | $0.00026 |
| Example 3 (DB) | ~700 | ~280 | $0.00029 |
| Example 4 (History) | ~200 | ~150 | $0.00011 |
| Example 5 (Complete) | ~850 | ~320 | $0.00035 |

**Total for all examples:** ~$0.00134

---

## Tips for Best Results

### 1. Write Clear Error Messages
❌ **Bad:** `Assertion failed`  
✅ **Good:** `Expected user.email to contain '@', but got 'john.example.com'`

### 2. Include Context in Tests
❌ **Bad:** Test without setup context  
✅ **Good:** Background section explaining test preconditions

### 3. Enable Evidence Collection
- Screenshots for UI tests
- Playwright traces for debugging
- Structured logs for analysis

### 4. Use Comparative Analysis for Novel Issues
- First time seeing failure? Compare analyzers
- Low confidence? Get second opinion
- Complex scenarios? Use both perspectives

### 5. Review Historical Patterns Weekly
- Identify flaky tests early
- Track test stability trends
- Prioritize fixes based on patterns

---

## Troubleshooting Demo Issues

### LLM Returns "Unknown"
**Cause:** Insufficient context or truly novel failure  
**Solution:** Enable more evidence sources, check error message clarity

### Low Confidence (<0.5)
**Cause:** Ambiguous failure, missing information  
**Solution:** Run comparative analysis, review historical patterns

### API Key Error
**Cause:** Invalid or missing API key  
**Solution:** 
```bash
echo $LLM_API_KEY  # Check if set
export LLM_API_KEY="sk-..."  # Set it
```

### Timeout Errors
**Cause:** LLM provider slow or unresponsive  
**Solution:** Increase timeout in appsettings.json or check network

---

## Next Steps

1. **Try it yourself**: Run the gateway with your tests
2. **Enable LLM**: Set API key and enable in config
3. **Analyze a failure**: Use real test failures
4. **Compare results**: See deterministic vs LLM
5. **Track patterns**: Let it run for a week, analyze trends

**Ready to revolutionize your test failure analysis! 🚀**
