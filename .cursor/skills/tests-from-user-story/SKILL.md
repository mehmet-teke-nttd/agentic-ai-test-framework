---
name: tests-from-user-story
description: >-
  Convert user stories with acceptance criteria into ReqnRoll feature files and step definition stubs.
  Parses As/I want/So that narratives and Given-When-Then acceptance criteria, generates Gherkin scenarios
  with metadata tags, and creates Playwright step definition boilerplate. Use when the user provides
  user stories, acceptance criteria, or asks to "generate tests from user story" or "create test cases from AC".
---

# Generate Tests from User Stories

This skill converts structured user stories with acceptance criteria into production-ready ReqnRoll feature files and C# step definition stubs for the Agentic AI Test Framework.

## Quick Start

When the user provides:
- A user story with acceptance criteria
- A document like `docs/demo/user-stories.md`
- A request like "Generate tests from this user story"

**Follow this workflow:**

1. **Parse the user story** — Extract story ID, persona, goal, benefit
2. **Parse acceptance criteria** — Extract AC ID, Given, When, Then clauses
3. **Map AC to scenarios** — One scenario per AC (or group related ACs)
4. **Assign metadata tags** — Category, Feature, Risk, Layer, Requirement
5. **Generate feature file** — Create `.feature` in `tests/UI.Tests/Features/`
6. **Generate step definitions** — Create `*Steps.cs` in `tests/UI.Tests/Steps/`
7. **Add traceability comments** — Link scenarios back to AC IDs

---

## User Story Input Format

The skill expects user stories in this structure:

```markdown
## US-XXX-NNN: Feature Name

**Story ID:** US-XXX-NNN
**Requirement ID:** REQ-AREA-NNN
**Feature:** PascalCaseFeatureName
**Risk:** High

### User Story

As a **[persona]**
I want to **[action]**
So that **[benefit]**

### Acceptance Criteria

| AC ID | Given | When | Then |
|-------|-------|------|------|
| AC-NNN-01 | context | action | expected result |
```

### Parsing Rules

1. **Story ID** → Used in scenario comments for traceability
2. **Requirement ID** → Maps to `@Requirement:` tag
3. **Feature name** → Maps to `@Feature:` tag and file naming
4. **Risk level** → Maps to `@Risk:` tag (default scenario risk; override per scenario if needed)
5. **Each AC row** → Becomes one Gherkin scenario (unless ACs are logically grouped)

---

## Acceptance Criteria → Scenario Mapping

### One AC = One Scenario (Default)

```gherkin
# AC-001-01: Enter valid registration details
@Category:Smoke @Feature:TextFieldRegistration @Risk:High @Layer:Ui @Requirement:REQ-DEMO-TEXT-001
Scenario: Enter valid registration details into text fields
  Given I am on the registration page
  When I enter valid name, email, and password
  Then the entered values should be visible in the respective text fields
```

### Grouping Related ACs (Optional)

Group ACs when they test the same flow sequentially:

```gherkin
Scenario: Complete dropdown selection flow
  # AC-002-03, AC-002-04, AC-002-05
  Given I am on the multi-select dropdown page
  When I select a country using visible text
  And I select a state using value attribute
  And I select a city using index
  Then all dropdowns should reflect the selected values
```

### Category Assignment from AC Type

| AC Pattern | Category | Notes |
|------------|----------|-------|
| Happy path, valid input | Smoke | Core functionality |
| Invalid input, errors | Regression | Negative testing |
| Boundary, edge cases | Regression | Edge case coverage |
| Empty/default state | Regression | Initial state verification |
| Security, data integrity | Smoke + Critical risk | Elevate risk level |

---

## Metadata Tag Requirements

Every scenario MUST include these tags:

```gherkin
@Category:<value> @Feature:<name> @Risk:<level> @Layer:Ui @Requirement:<id>
```

### Tag Reference

| Tag | Source | Values |
|-----|--------|--------|
| `@Category` | AC type | `Smoke`, `Regression`, `Performance` |
| `@Feature` | User story Feature field | PascalCase |
| `@Risk` | User story Risk field | `Low`, `Medium`, `High`, `Critical` |
| `@Layer` | Always `Ui` for UI stories | `Ui` |
| `@Requirement` | User story Requirement ID | `REQ-<AREA>-<NUM>` |

### Feature-Level Tags

Apply the highest-risk scenario's tags at the Feature level:

```gherkin
@Category:Smoke @Feature:FormValidation @Risk:Critical @Layer:Ui @Requirement:REQ-DEMO-FORM-001
Feature: Registration Form Validation
  As a system administrator I want to ensure the registration form validates user input correctly
  So that only valid data is accepted and users receive clear feedback on errors
```

---

## File Structure and Naming

### Feature Files

- **Location**: `tests/UI.Tests/Features/` (or `Features/Demo/` for demo stories)
- **Naming**: `<FeatureName>.feature` (PascalCase from user story Feature field)
- **Example**: `TextFieldRegistration.feature`, `DropdownProductSelection.feature`

### Step Definition Files

- **Location**: `tests/UI.Tests/Steps/` (or `Steps/Demo/` for demo stories)
- **Naming**: `<FeatureName>Steps.cs`
- **Example**: `TextFieldRegistrationSteps.cs`

---

## Feature File Template

```gherkin
@Category:Smoke @Feature:<FeatureName> @Risk:<Risk> @Layer:Ui @Requirement:<ReqId>
Feature: <Human-readable feature title>
  <User story "I want" clause>
  <User story "So that" clause>

  Background:
    Given a Chromium browser is available

  # AC-<id>: <short AC description>
  @Category:Smoke @Risk:<Risk>
  Scenario: <Descriptive scenario name from AC>
    Given <from AC Given column>
    When <from AC When column>
    Then <from AC Then column>

  # AC-<id>: <short AC description>
  @Category:Regression @Risk:<Risk>
  Scenario: <Next scenario>
    ...
```

### Background Rules

- Include `Given a Chromium browser is available` when browser setup is shared
- Include common navigation (e.g., `Given I am on the registration page`) if ALL scenarios start there
- Do NOT put scenario-specific setup in Background

---

## Step Definition Template

```csharp
using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps;

[Binding]
public sealed class <FeatureName>Steps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    [Given("a Chromium browser is available")]
    public async Task GivenAChromiumBrowserIsAvailable()
    {
        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }
    }

    [Given("I am on the <page> page")]
    public async Task GivenIAmOnThePage()
    {
        if (_session is null)
        {
            await GivenAChromiumBrowserIsAvailable();
        }

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await RequireSession().Page.GotoAsync($"{applicationUrl}/<route>");
    }

    [When("I <action>")]
    public async Task WhenIAction()
    {
        // TODO: Implement action — map to AC When clause
    }

    [Then("I should <expected>")]
    public async Task ThenIShouldExpected()
    {
        // TODO: Implement assertion — map to AC Then clause
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

---

## Natural Language → Step Mapping

Convert AC table columns to Gherkin steps:

| AC Column | Gherkin Keyword | Example |
|-----------|----------------|---------|
| Given | `Given` | `Given I am on the registration page` |
| When | `When` / `And` | `When I enter valid name, email, and password` |
| Then | `Then` / `And` | `Then the entered values should be visible in the respective text fields` |

### Parameterization

Use `{string}` parameters for variable data in steps:

```gherkin
When I enter name "John Doe"
And I enter email "john@example.com"
```

```csharp
[When("I enter name {string}")]
public async Task WhenIEnterName(string name) { ... }
```

---

## Traceability Requirements

Every generated scenario MUST include a comment linking to its AC:

```gherkin
  # AC-001-01: Enter valid registration details
  Scenario: Enter valid registration details into text fields
```

When multiple ACs are grouped:

```gherkin
  # AC-002-03, AC-002-04, AC-002-05: Cascading dropdown selection
  Scenario: Select country state and city in cascading dropdowns
```

---

## Application URL Configuration

Load the base URL from `tests/UI.Tests/appsettings.json`:

```json
{
  "Application": {
    "BaseUrl": "https://demoapps.qspiders.com/"
  }
}
```

Use `ApplicationUrlResolver.Load(settingsPath)` in step definitions.

---

## Example: Complete Conversion

### Input User Story

```markdown
## US-DEMO-003: Checkbox Notification Preferences

**Requirement ID:** REQ-DEMO-CHK-001
**Feature:** CheckboxPreferences
**Risk:** Medium

As a **customer** I want to **select notification checkboxes** So that **I receive updates**

| AC ID | Given | When | Then |
| AC-003-01 | on checkbox page | select Email | Email checkbox checked |
| AC-003-04 | on checkbox page | select all checkboxes | all checked |
```

### Generated Feature File

`tests/UI.Tests/Features/Demo/DemoCheckboxes.feature`:

```gherkin
@Category:Smoke @Feature:CheckboxPreferences @Risk:Medium @Layer:Ui @Requirement:REQ-DEMO-CHK-001
Feature: Checkbox Notification Preferences
  As a customer I want to select notification checkboxes
  So that I receive updates on my preferred platforms

  Background:
    Given a Chromium browser is available

  # AC-003-01
  @Category:Smoke
  Scenario: Select Email notification checkbox
    Given I am on the checkbox preferences page
    When I select the "Email" notification checkbox
    Then the "Email" checkbox should be checked

  # AC-003-04
  @Category:Regression
  Scenario: Select all notification and recommendation checkboxes
    Given I am on the checkbox preferences page
    When I select all notification and recommendation checkboxes
    Then all checkboxes should be checked
```

---

## Checklist for Generated Tests

Before finalizing, verify:

- [ ] Every AC has a corresponding scenario (or grouped scenario with AC comment)
- [ ] All scenarios have complete metadata tags
- [ ] Feature-level tags match highest-risk scenario
- [ ] User story narrative appears in Feature description
- [ ] Background section avoids scenario-specific setup
- [ ] Step definitions follow Playwright session pattern
- [ ] `[AfterScenario]` includes evidence capture
- [ ] Requirement IDs trace back to user story
- [ ] Scenario names are descriptive and unique
- [ ] Both Smoke and Regression categories are represented

---

## Integration with Other Skills

| Skill | When to Use Together |
|-------|---------------------|
| `expand-test-idea` | After generating from user story, expand with additional edge cases |
| `ui-tests-from-screenshot` | When user story lacks UI detail, use screenshots to refine selectors |

**Workflow:**
```
User Story + AC → tests-from-user-story → Base test suite
                                          ↓
                              expand-test-idea → Additional edge cases
                              ui-tests-from-screenshot → Refined selectors
```

---

## When to Use This Skill

✅ User provides user stories with acceptance criteria  
✅ Converting requirements documents to test cases  
✅ Sprint planning output needs automated test coverage  
✅ Traceability from requirements to tests is required  
✅ Demo or POC showing requirements-driven testing  

## When NOT to Use This Skill

❌ Only a brief test idea (use `expand-test-idea`)  
❌ Only screenshots/mockups (use `ui-tests-from-screenshot`)  
❌ API or integration tests (different layer)  
❌ User story lacks Given-When-Then acceptance criteria  
