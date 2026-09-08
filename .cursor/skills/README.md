# UI Test Generation Skills - Quick Reference

This project includes three powerful skills for generating UI functional tests automatically.

## Skills Overview

### 1. `tests-from-user-story`
**Purpose:** Convert user stories with acceptance criteria into ReqnRoll feature files

**When to use:**
- You have user stories with Given-When-Then acceptance criteria
- Requirements traceability from story to test is needed
- Converting sprint planning output to automated tests

**Example usage:**
```
"Generate tests from this user story"
"Create test cases from the acceptance criteria in docs/demo/user-stories.md"
"Convert US-DEMO-001 to ReqnRoll tests"
```

**What it generates:**
- Feature file with one scenario per acceptance criterion
- Metadata tags linked to Requirement IDs
- AC traceability comments on each scenario
- Step definition stubs in C#

---

### 2. `expand-test-idea`
**Purpose:** Expand brief test descriptions into comprehensive feature files

**When to use:**
- You have a test idea in mind
- You need multiple scenarios for a feature
- You want positive, negative, and edge case coverage

**Example usage:**
```
"Expand test idea: Test the shopping cart functionality"
"Generate tests for user registration"
"I need comprehensive tests for the checkout flow"
```

**What it generates:**
- Complete `.feature` file with metadata tags
- Multiple scenarios (smoke, regression, edge cases)
- Scenario Outlines for data-driven tests
- Step definition stubs in C#

---

### 3. `ui-tests-from-screenshot`
**Purpose:** Analyze UI screenshots to generate test coverage

**When to use:**
- You have design mockups or screenshots
- You need to test an existing screen
- You want to ensure all UI elements are covered

**Example usage:**
```
"Generate tests from this login screen" (attach screenshot)
"Create test cases based on designs/checkout-form.png"
"Analyze this wireframe and create tests"
```

**What it analyzes:**
- Form fields and validation requirements
- Buttons and interactive elements
- Navigation and links
- Error message areas
- Layout and structure

**What it generates:**
- Feature file based on identified elements
- Risk-appropriate metadata tags
- Comprehensive step definitions
- Accessibility test scenarios

---

## How to Use These Skills

### Method 1: Direct Invocation
Simply mention the task and the skill will be automatically applied:

```
"I want to test the login form" → expand-test-idea activates
"Generate tests from this screenshot" → ui-tests-from-screenshot activates
```

### Method 2: Explicit Call
You can explicitly name the skill:

```
"Use expand-test-idea skill to generate tests for checkout"
"Apply ui-tests-from-screenshot to analyze this form"
```

---

## Generated Test Structure

Both skills generate tests following this project's conventions:

### Feature File Format
```gherkin
@Category:<value> @Feature:<name> @Risk:<level> @Layer:Ui @Requirement:<id>
Feature: Descriptive feature name
  Feature description explaining the scope

  Background:
    Given common setup steps

  @Category:Smoke @Risk:High
  Scenario: Happy path scenario
    Given initial state
    When user action
    Then expected result

  @Category:Regression @Risk:Medium
  Scenario: Validation scenario
    Given initial state
    When invalid action
    Then validation error shown
```

### Step Definition Format
```csharp
[Binding]
public sealed class FeatureSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;
    
    // Step definitions
    
    [AfterScenario]
    public async Task CaptureEvidenceAndCloseAsync()
    {
        // Automatic cleanup and evidence capture
    }
}
```

---

## Metadata Tags Reference

All generated tests include proper metadata:

| Tag | Purpose | Values |
|-----|---------|--------|
| `@Category` | Test execution grouping | `Smoke`, `Regression`, `Performance` |
| `@Feature` | Feature identification | PascalCase name |
| `@Risk` | Impact assessment | `Low`, `Medium`, `High`, `Critical` |
| `@Layer` | Test layer | `Ui` (always for these skills) |
| `@Requirement` | Traceability | `REQ-<AREA>-<NUM>` |

---

## Example Workflows

### Workflow 1: Feature Idea → Complete Tests
```
User: "I need tests for a password reset feature"

AI (using expand-test-idea):
1. Generates PasswordReset.feature with:
   - Request password reset (smoke)
   - Invalid email validation (regression)
   - Reset link expiration (regression)
   - Successfully reset password (smoke)
   - Scenario Outline for email format validation

2. Generates PasswordResetSteps.cs with:
   - All step definitions
   - Playwright session management
   - Evidence capture setup
```

### Workflow 2: Screenshot → Test Coverage
```
User: (attaches checkout-form.png) "Generate tests for this"

AI (using ui-tests-from-screenshot):
1. Analyzes screenshot:
   - Identifies: email field, card number, CVV, expiry, submit button
   
2. Generates CheckoutPayment.feature with:
   - Valid payment submission
   - Invalid card number format
   - Expired card validation
   - Missing required fields
   - Security validation (CVV)

3. Generates CheckoutPaymentSteps.cs with:
   - Proper selectors for each field
   - Validation assertions
   - Error message checks
```

### Workflow 3: Combined Approach
```
User: "Test the login form" (provides screenshot)

AI:
1. Uses ui-tests-from-screenshot to identify elements
2. Uses expand-test-idea to ensure comprehensive coverage
3. Generates complete test suite with both visual and logical coverage
```

---

## Tips for Best Results

### For `expand-test-idea`:
- ✅ Be specific: "Test user registration with email verification"
- ✅ Mention domain: "E-commerce checkout flow"
- ❌ Too vague: "Test the app"

### For `ui-tests-from-screenshot`:
- ✅ Provide clear, high-resolution screenshots
- ✅ Include the entire screen/component
- ✅ Show error states if available
- ❌ Blurry or partial screenshots

---

## Integration with Your Framework

Both skills are designed specifically for this project:
- Use ReqnRoll (Gherkin) syntax
- Generate Playwright-based step definitions
- Include automatic evidence capture
- Follow the gateway execution model
- Support CI/CD integration
- Enable agent-based failure analysis

---

## Next Steps

1. **Try the skills:** Just ask to generate tests!
2. **Review generated code:** Ensure it matches your needs
3. **Customize selectors:** Update with actual application IDs/classes
4. **Run tests:** `dotnet test tests/UI.Tests/UI.Tests.csproj`
5. **Iterate:** Provide feedback to refine generated tests

---

## Need Help?

Ask questions like:
- "Show me an example using expand-test-idea"
- "How do I customize the generated step definitions?"
- "Can you generate tests for multiple screens at once?"
- "How do I change the risk level for generated tests?"

The skills will adapt to your specific needs!
