# UI Testing Demo Guide

This guide walks through the complete UI testing demo workflow using [demoapps.qspiders.com](https://demoapps.qspiders.com/) as the target application.

## Demo Overview

This demo showcases a **requirements-driven testing workflow**:

```
User Story + Acceptance Criteria
        ↓
  tests-from-user-story skill
        ↓
  ReqnRoll .feature files
        ↓
  Playwright step definitions
        ↓
  Test execution + evidence capture
        ↓
  Results in dashboard / TRX output
```

### What This Demo Proves

- User stories with acceptance criteria can be automatically converted to executable UI tests
- Tests follow framework conventions (metadata tags, evidence capture, Playwright sessions)
- Traceability is maintained from requirement ID → scenario → step definitions
- The workflow integrates with the existing Agentic AI Test Framework

---

## Prerequisites

### Environment Setup

1. **.NET 8 SDK** installed
2. **Chromium browser** for Playwright:
   ```powershell
   pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
   ```
3. **Application URL** configured in `tests/UI.Tests/appsettings.json`:
   ```json
   {
     "Application": {
       "BaseUrl": "https://demoapps.qspiders.com/"
     }
   }
   ```

### Build the Test Project

```powershell
dotnet build tests/UI.Tests/UI.Tests.csproj
```

---

## User Stories

Four user stories define the demo scope. Full details are in [user-stories.md](./user-stories.md).

| Story ID | Feature | Requirement ID | Scenarios |
|----------|---------|----------------|-----------|
| US-DEMO-001 | Text Field Registration | REQ-DEMO-TEXT-001 | 5 |
| US-DEMO-002 | Multi-Select Dropdown | REQ-DEMO-DROP-001 | 4 |
| US-DEMO-003 | Checkbox Preferences | REQ-DEMO-CHK-001 | 6 |
| US-DEMO-004 | Form Validation | REQ-DEMO-FORM-001 | 5 |

### Example User Story

**US-DEMO-003: Checkbox Notification Preferences**

> As a **customer completing an order**  
> I want to **select my notification and product preference checkboxes**  
> So that **I receive updates on my preferred platforms and product recommendations**

**Sample Acceptance Criterion (AC-003-01):**

| Given | When | Then |
|-------|------|------|
| I am on the checkbox preferences page | I select Email notification checkbox | The Email checkbox should be checked |

---

## Generated Tests

### Feature Files

Located in `tests/UI.Tests/Features/Demo/`:

| File | User Story | URL Path |
|------|------------|----------|
| [DemoTextFields.feature](../../tests/UI.Tests/Features/Demo/DemoTextFields.feature) | US-DEMO-001 | `/ui/textcaret-1` |
| [DemoDropdowns.feature](../../tests/UI.Tests/Features/Demo/DemoDropdowns.feature) | US-DEMO-002 | `/ui/dropdown/multiSelect?sublist=1` |
| [DemoCheckboxes.feature](../../tests/UI.Tests/Features/Demo/DemoCheckboxes.feature) | US-DEMO-003 | `/ui/checkbox?sublist=0` |
| [DemoFormValidation.feature](../../tests/UI.Tests/Features/Demo/DemoFormValidation.feature) | US-DEMO-004 | `/ui/textcaret-1` |

### Step Definitions

Located in `tests/UI.Tests/Steps/Demo/`:

| File | Purpose |
|------|---------|
| `DemoTextFieldsSteps.cs` | Text input, placeholder, default value, capture |
| `DemoDropdownsSteps.cs` | Multi-select and cascading dropdown interactions |
| `DemoCheckboxesSteps.cs` | Checkbox selection and state verification |
| `DemoFormValidationSteps.cs` | Form validation error assertions |

---

## Execution Guide

### Run All Demo Tests

```powershell
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "Feature=TextFieldRegistration|Feature=DropdownProductSelection|Feature=CheckboxPreferences|Feature=FormValidation"
```

### Run by Category

```powershell
# Smoke tests only
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "Category=Smoke&Feature=CheckboxPreferences"

# Regression tests only
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "Category=Regression"
```

### Run a Single Feature

```powershell
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "Feature=CheckboxPreferences"
```

### Run with TRX Output (for dashboard)

```powershell
dotnet test tests/UI.Tests/UI.Tests.csproj `
  --filter "Feature=CheckboxPreferences" `
  --logger "trx;LogFileName=demo-results.trx"
```

---

## Skill Usage

### Using `tests-from-user-story`

The skill at `.cursor/skills/tests-from-user-story/SKILL.md` converts user stories to tests.

**How to invoke:**

```
Generate tests from user story US-DEMO-001 in docs/demo/user-stories.md
```

```
Convert this user story to ReqnRoll tests:
As a shopper I want to select products from dropdowns
So that I can configure my order

AC: Given on dropdown page, When I select a country, Then country is selected
```

**What the skill produces:**

1. `.feature` file with one scenario per acceptance criterion
2. Metadata tags (`@Category`, `@Feature`, `@Risk`, `@Layer`, `@Requirement`)
3. AC traceability comments on each scenario
4. C# step definition stubs with Playwright session management

### Skill Comparison

| Skill | Input | Best For |
|-------|-------|----------|
| `tests-from-user-story` | User stories + AC | Requirements traceability |
| `expand-test-idea` | Brief test descriptions | Exploratory coverage expansion |
| `ui-tests-from-screenshot` | Screenshots/mockups | Visual UI analysis |

### Recommended Workflow

```
1. Write user story with acceptance criteria
2. Apply tests-from-user-story → base test suite
3. Apply expand-test-idea → additional edge cases
4. Apply ui-tests-from-screenshot → refine selectors (optional)
5. Run tests and iterate on step definitions
```

---

## Demo Presentation Flow

### Step 1: Show the User Story (2 min)

Open [user-stories.md](./user-stories.md) and walk through US-DEMO-003:
- Persona, goal, benefit
- Acceptance criteria table with Given-When-Then

### Step 2: Apply the Skill (3 min)

In Cursor, ask:
```
Generate tests from US-DEMO-003 in docs/demo/user-stories.md
```

Show the generated `DemoCheckboxes.feature` and `DemoCheckboxesSteps.cs`.

### Step 3: Review Generated Code (3 min)

Highlight:
- Metadata tags for filtering and risk assessment
- `# AC-003-01` traceability comments
- Background section for shared setup
- Playwright session management and evidence capture

### Step 4: Execute Tests (5 min)

```powershell
dotnet test tests/UI.Tests/UI.Tests.csproj --filter "Feature=CheckboxPreferences"
```

Show pass/fail results and any captured screenshots on failure.

### Step 5: Show Integration (2 min)

- TRX output for dashboard ingestion
- Category/Risk tags enabling selective test execution
- Requirement IDs linking back to user stories

---

## Expected Results

| Demo Step | Expected Outcome |
|-----------|------------------|
| User story review | Clear Given-When-Then acceptance criteria |
| Skill application | Feature file + step definitions generated |
| Build | `dotnet build` succeeds with 0 errors |
| Test execution | Tests run against demoapps.qspiders.com |
| Evidence capture | Screenshots/traces saved on failure |
| Traceability | REQ-DEMO-* IDs link stories to scenarios |

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Chromium unavailable | Run `playwright.ps1 install chromium` |
| Tests timeout | Check network access to demoapps.qspiders.com |
| Selector not found | Inspect live page and update step definitions |
| Ambiguous step binding | Use unique step text or consolidate bindings |
| Site layout changed | Update selectors in `Steps/Demo/` files |

---

## Next Steps

1. **Refine selectors** — Inspect live pages and update step definitions with stable selectors
2. **Add more stories** — Extend coverage to Radio Buttons, Slider, Web Table
3. **CI integration** — Add demo tests to pipeline with TRX reporting
4. **Dashboard** — View results in TestDashboard after test runs
