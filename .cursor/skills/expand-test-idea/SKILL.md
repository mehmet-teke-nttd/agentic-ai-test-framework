---
name: expand-test-idea
description: >-
  Expand brief test descriptions into comprehensive ReqnRoll feature files with multiple scenarios (positive, negative, edge cases). 
  Generates proper Gherkin syntax with metadata tags (@Category, @Feature, @Risk, @Layer, @Requirement) and creates step definition stubs. 
  Use when the user provides a test idea, feature description, or asks to "generate tests for" something.
---

# Expand Test Idea to Comprehensive UI Tests

This skill takes brief test descriptions and expands them into complete, production-ready ReqnRoll feature files for the Agentic AI Test Framework.

## Quick Start

When the user provides a test idea like:
- "Test the login form"
- "Generate tests for checkout"
- "I need tests for user registration"

**Follow this workflow:**

1. **Understand the scope** - Identify the feature domain
2. **Generate feature file** - Create `.feature` file in `tests/UI.Tests/Features/`
3. **Include multiple scenarios** - Cover positive, negative, and edge cases
4. **Apply metadata** - Use proper tags for categorization
5. **Create step definitions** - Generate stub implementation in `tests/UI.Tests/Steps/`

---

## Metadata Tag Requirements

Every scenario MUST include these tags:

```gherkin
@Category:<value> @Feature:<name> @Risk:<level> @Layer:Ui @Requirement:<id>
```

### Tag Reference

| Tag | Values | When to Use |
|-----|--------|-------------|
| `@Category` | `Smoke`, `Regression`, `Performance` | Smoke=critical paths, Regression=thorough coverage |
| `@Feature` | PascalCase feature name | Match the feature being tested |
| `@Risk` | `Low`, `Medium`, `High`, `Critical` | Impact if this functionality fails |
| `@Layer` | `Ui` (always for these tests) | Test layer identifier |
| `@Requirement` | `REQ-<AREA>-<NUM>` | Traceability ID (e.g., REQ-UI-001) |

**Risk Level Guidelines:**
- **Critical**: Security, payments, data loss scenarios
- **High**: Core functionality, user-blocking issues
- **Medium**: Important but non-blocking features
- **Low**: Nice-to-have, cosmetic, minor issues

---

## Scenario Coverage Pattern

For each feature, generate scenarios covering:

### 1. **Happy Path (Smoke)**
```gherkin
@Category:Smoke @Feature:Login @Risk:High @Layer:Ui @Requirement:REQ-AUTH-001
Scenario: Successful login with valid credentials
  Given I am on the login page
  When I enter valid username "user@example.com"
  And I enter valid password
  And I click the login button
  Then I should be redirected to the dashboard
  And I should see a welcome message
```

### 2. **Negative Cases (Regression)**
```gherkin
@Category:Regression @Feature:Login @Risk:High @Layer:Ui @Requirement:REQ-AUTH-002
Scenario: Login fails with invalid credentials
  Given I am on the login page
  When I enter username "user@example.com"
  And I enter incorrect password
  And I click the login button
  Then I should see an error message "Invalid credentials"
  And I should remain on the login page
```

### 3. **Edge Cases (Regression)**
```gherkin
@Category:Regression @Feature:Login @Risk:Medium @Layer:Ui @Requirement:REQ-AUTH-003
Scenario: Login form validation with empty fields
  Given I am on the login page
  When I leave the username field empty
  And I leave the password field empty
  And I click the login button
  Then I should see validation errors
  And the form should not be submitted
```

### 4. **Data-Driven Tests (When applicable)**
```gherkin
@Category:Regression @Feature:Login @Risk:Medium @Layer:Ui @Requirement:REQ-AUTH-004
Scenario Outline: Email field format validation
  Given I am on the login page
  When I enter "<email>" in the username field
  Then I should see "<result>"
  
  Examples:
    | email              | result                |
    | valid@test.com     | accepted              |
    | invalid-email      | validation error      |
    | @domain.com        | validation error      |
    | test@              | validation error      |
```

---

## File Structure and Naming

### Feature Files
- **Location**: `tests/UI.Tests/Features/`
- **Naming**: `<FeatureName>.feature` (PascalCase)
- **Examples**: `Login.feature`, `Checkout.feature`, `UserRegistration.feature`

### Step Definition Files
- **Location**: `tests/UI.Tests/Steps/`
- **Naming**: `<FeatureName>Steps.cs` (PascalCase)
- **Examples**: `LoginSteps.cs`, `CheckoutSteps.cs`, `UserRegistrationSteps.cs`

---

## Step Definition Template

Generate step definition stubs using this template:

```csharp
using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps;

[Binding]
public sealed class <FeatureName>Steps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    [Given("I am on the <page> page")]
    public async Task GivenIAmOnThePage()
    {
        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }
        
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await _session.Page.GotoAsync($"{applicationUrl}/<page-route>");
    }

    [When("I <action>")]
    public async Task WhenIPerformAction()
    {
        // TODO: Implement action
        await RequireSession().Page.ClickAsync("selector");
    }

    [Then("I should see <expected>")]
    public async Task ThenIShouldSeeExpected()
    {
        // TODO: Implement assertion
        var element = await RequireSession().Page.Locator("selector").TextContentAsync();
        Assert.That(element, Does.Contain("expected text"));
    }

    [AfterScenario]
    public async Task CaptureEvidenceAndCloseAsync()
    {
        if (_session is null) return;
        if (scenarioContext.TestError is not null)
        {
            await _session.CaptureFailureAsync(scenarioContext.ScenarioInfo.Title);
        }
        await _session.DisposeAsync();
    }

    private PlaywrightScenarioSession RequireSession() =>
        _session ?? throw new InvalidOperationException("Browser session was not initialized.");
}
```

### Key Implementation Notes:

1. **Always check for Chromium availability** - Use graceful ignore
2. **Evidence capture on failure** - Automatic screenshots/traces via `CaptureFailureAsync`
3. **Proper cleanup** - Always dispose session in `[AfterScenario]`
4. **Use ApplicationUrlResolver** - Load base URL from `appsettings.json`

---

## Common UI Test Patterns

### Forms
```gherkin
Scenario: Submit form with valid data
  Given I am on the <form> page
  When I fill in "<field>" with "<value>"
  And I click the submit button
  Then the form should be submitted successfully
  And I should see a confirmation message
```

### Navigation
```gherkin
Scenario: Navigate to page via menu
  Given I am on the home page
  When I click the "<menu-item>" menu item
  Then I should be on the "<page>" page
  And the page should be fully loaded
```

### Validation
```gherkin
Scenario: Field validation prevents submission
  Given I am on the form page
  When I enter invalid data in required fields
  And I attempt to submit
  Then validation errors should be displayed
  And the form should not be submitted
```

### Error Handling
```gherkin
Scenario: Graceful error handling
  Given I am on the page
  When an error occurs during operation
  Then I should see a user-friendly error message
  And the application should remain responsive
```

---

## Example: Complete Feature File

**User request:** "Generate tests for a shopping cart"

**Generated output:**

`tests/UI.Tests/Features/ShoppingCart.feature`:
```gherkin
@Category:Smoke @Feature:ShoppingCart @Risk:High @Layer:Ui @Requirement:REQ-CART-001
Feature: Shopping Cart Management
  Users should be able to add, remove, and update items in their shopping cart
  The cart should persist across sessions and calculate totals correctly

  Background:
    Given a Chromium browser is available
    And I am logged in as a customer

  @Category:Smoke
  Scenario: Add item to empty cart
    Given my shopping cart is empty
    When I add a product to the cart
    Then the cart should contain 1 item
    And the cart total should be updated

  @Category:Regression
  Scenario: Add multiple items to cart
    Given my shopping cart is empty
    When I add 3 different products to the cart
    Then the cart should contain 3 items
    And each item should be listed separately

  @Category:Regression @Risk:Medium
  Scenario: Remove item from cart
    Given I have 2 items in my cart
    When I remove one item
    Then the cart should contain 1 item
    And the cart total should be recalculated

  @Category:Regression
  Scenario: Update item quantity
    Given I have a product in my cart with quantity 1
    When I change the quantity to 3
    Then the cart should show quantity 3 for that item
    And the item subtotal should be multiplied by 3

  @Category:Smoke @Risk:Critical
  Scenario: Cart persists across sessions
    Given I have items in my cart
    When I log out and log back in
    Then my cart should still contain the same items

  @Category:Regression @Risk:Medium
  Scenario: Empty cart shows appropriate message
    Given my shopping cart is empty
    When I view my cart
    Then I should see "Your cart is empty" message
    And I should see a link to continue shopping

  @Category:Regression
  Scenario Outline: Cart total calculation is accurate
    Given my cart is empty
    When I add an item priced at <price> with quantity <quantity>
    Then the subtotal should be <expected>

    Examples:
      | price | quantity | expected |
      | 10.00 | 1        | 10.00    |
      | 10.00 | 3        | 30.00    |
      | 25.50 | 2        | 51.00    |

  @Category:Regression @Risk:High
  Scenario: Cannot proceed to checkout with empty cart
    Given my shopping cart is empty
    When I attempt to proceed to checkout
    Then I should see a message "Add items to cart first"
    And the checkout button should be disabled
```

---

## Checklist for Generated Tests

Before finalizing, verify:

- [ ] All scenarios have complete metadata tags
- [ ] Feature-level tags include highest risk scenario
- [ ] Both smoke and regression tests are included
- [ ] Negative and edge cases are covered
- [ ] Scenario names are descriptive and unique
- [ ] Given-When-Then structure is clear
- [ ] Data-driven tests use Scenario Outline where appropriate
- [ ] Background section is used for common setup
- [ ] Risk levels are appropriate
- [ ] Requirement IDs follow convention

---

## Tips for Quality Test Generation

1. **Start broad, then specific** - Cover main flows before edge cases
2. **Think like a user** - What would they try? What might break?
3. **Include failure scenarios** - Test error handling and validation
4. **Data-driven when patterns emerge** - Use Scenario Outline for variations
5. **Descriptive scenario names** - Should explain what's being tested
6. **Background for common setup** - Avoid repetitive Given steps
7. **Tag appropriately** - Risk and Category guide test execution priority

---

## Common Test Ideas → Generated Coverage

| User Input | Generated Scenarios |
|------------|---------------------|
| "Test login" | Valid login, invalid credentials, empty fields, locked account, password reset link |
| "Test checkout" | Complete purchase, payment validation, address validation, cart empty, inventory check |
| "Test search" | Valid search, no results, special characters, filters, pagination, sort options |
| "Test profile" | View profile, edit details, change password, upload photo, delete account |

---

## Integration with Framework

The generated tests work seamlessly with:

- **Gateway execution**: Tests are discoverable via `discover_tests` tool
- **Evidence collection**: Automatic screenshots/traces on failure
- **Test filtering**: Category/Risk/Layer tags enable selective execution
- **CI/CD**: Tests run via `dotnet test` with proper TRX output
- **Agent analysis**: Failure analyzer can classify issues automatically

---

## When NOT to Use This Skill

Don't use this skill for:
- API/Integration tests (use different layer tags)
- Unit tests (not Gherkin-based)
- Performance tests (require specialized setup)
- Tests that require MCP or external tool calls

For those cases, create tests manually or use appropriate tooling.
