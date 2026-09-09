# Page Object Model Pattern - Mandatory Framework Rule

## 🎯 Core Principle

**This framework STRICTLY follows the Page Object Model (POM) pattern.**

All UI test automation MUST separate test logic from UI interaction logic.

---

## ✅ REQUIRED Architecture

### 🏷️ **Naming Convention (MANDATORY)**

**All related files MUST use the same base name:**

```
{FeatureName}.feature          ← Feature file
{FeatureName}Steps.cs          ← Step definitions
{FeatureName}Page.cs           ← Page Object
```

**Example:**
```
DemoButtonInteraction.feature           ✅
DemoButtonInteractionSteps.cs           ✅
DemoButtonInteractionPage.cs            ✅

NOT:
DemoButtonInteraction.feature           ✅
DemoButtonInteractionSteps.cs           ✅
ButtonTestingPage.cs                    ❌ WRONG - name mismatch
```

**Checkout Example:**
```
CompleteEcommerceCheckout.feature       ← Not currently following pattern
RegistrationSteps.cs                    ← Part 1
PaymentSelectionSteps.cs                ← Part 2
RegistrationPage.cs                     ← Matches RegistrationSteps ✅
PaymentSelectionPage.cs                 ← Matches PaymentSelectionSteps ✅
```

### 1. **Step Definitions** (`tests/UI.Tests/Steps/`)
- ✅ **NO direct Playwright calls allowed**
- ✅ Use Page Objects for ALL UI interactions
- ✅ Focus on test orchestration and business logic only
- ✅ Handle test setup, teardown, and assertions

**Allowed in Steps:**
```csharp
// ✅ CORRECT: Using Page Object
await _registrationPage.EnterNameAsync(name);
await _buttonPage.ClickYesButtonAsync();

// ✅ CORRECT: Browser session management
_session = await PlaywrightScenarioSession.TryStartAsync();
await _session.Page.GotoAsync(url);  // Initial navigation only

// ✅ CORRECT: NUnit assertions
Assert.That(result, Is.True);
```

**FORBIDDEN in Steps:**
```csharp
// ❌ WRONG: Direct Playwright calls
await _session.Page.ClickAsync("button");
await page.FillAsync("input", "text");
await page.Locator("div").TextContentAsync();

// ❌ WRONG: Direct selectors
var element = page.GetByRole(AriaRole.Button);
```

### 2. **Page Objects** (`tests/UI.Tests/PageObjects/`)
- ✅ **MUST match feature/step names**: `{FeatureName}Page.cs`
- ✅ Class name MUST be `{FeatureName}Page`
- ✅ Encapsulate ALL UI interactions
- ✅ Define ALL selectors as constants
- ✅ Provide meaningful method names
- ✅ Handle waits and element states
- ✅ Return typed results for assertions

**Required Structure:**
```csharp
using Microsoft.Playwright;

namespace UI.Tests.PageObjects;

/// <summary>
/// Page Object for [Page/Feature Name]
/// Encapsulates all interactions with [describe UI area]
/// </summary>
public sealed class ExamplePage(IPage page)
{
    // ✅ All selectors as constants
    private const string SubmitButtonSelector = "button[type='submit']";
    private const string NameInputSelector = "input[name='username']";
    
    // ✅ Navigation methods
    public async Task NavigateToAsync(string url)
    {
        await page.GotoAsync(url);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    // ✅ Interaction methods
    public async Task EnterNameAsync(string name)
    {
        await page.FillAsync(NameInputSelector, name);
    }
    
    public async Task ClickSubmitAsync()
    {
        var button = page.Locator(SubmitButtonSelector);
        await button.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await button.ClickAsync();
    }
    
    // ✅ Query methods (return values for assertions)
    public async Task<string?> GetConfirmationTextAsync()
    {
        var element = page.Locator(".confirmation");
        return await element.TextContentAsync();
    }
    
    // ✅ Verification methods (encapsulate complex checks)
    public async Task VerifyPageLoadedAsync()
    {
        await page.WaitForSelectorAsync(SubmitButtonSelector, 
            new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }
}
```

---

## 📋 Implementation Checklist

When creating or modifying UI tests, ensure:

- [ ] **Naming Convention**: Feature, Steps, and Page Object use same base name
  - [ ] `{FeatureName}.feature`
  - [ ] `{FeatureName}Steps.cs` with class `{FeatureName}Steps`
  - [ ] `{FeatureName}Page.cs` with class `{FeatureName}Page`
- [ ] Page Object class created in `tests/UI.Tests/PageObjects/`
- [ ] All selectors defined as `private const string` in Page Object
- [ ] All UI interactions (click, fill, select, etc.) in Page Object methods
- [ ] Step definitions use `using UI.Tests.PageObjects;`
- [ ] Step definitions call Page Object methods only
- [ ] Step definitions initialize Page Object: `_pageObject = new {FeatureName}Page(RequireSession().Page);`
- [ ] NO direct `page.Click()`, `page.Fill()`, `page.Locator()` in step definitions
- [ ] Assertions in step definitions, not in Page Objects
- [ ] Page Object methods have meaningful names describing actions
- [ ] Wait strategies handled in Page Objects

---

## 🏗️ File Structure

```
tests/UI.Tests/
├── PageObjects/
│   ├── RegistrationPage.cs              ✅ Matches RegistrationSteps
│   ├── PaymentSelectionPage.cs          ✅ Matches PaymentSelectionSteps
│   ├── DemoButtonInteractionPage.cs     ✅ Matches DemoButtonInteractionSteps
│   └── {FeatureName}Page.cs             ← Naming convention
│
├── Steps/
│   ├── Checkout/
│   │   ├── RegistrationSteps.cs         ✅ Matches RegistrationPage
│   │   ├── PaymentSelectionSteps.cs     ✅ Matches PaymentSelectionPage
│   │   └── CommonSteps.cs               ✅ Shared steps
│   └── Demo/
│       ├── DemoButtonInteractionSteps.cs ✅ Matches DemoButtonInteractionPage
│       └── {FeatureName}Steps.cs        ← Naming convention
│
└── Features/
    ├── Demo/
    │   ├── DemoButtonInteraction.feature ✅ Matches Steps & Page
    │   └── {FeatureName}.feature        ← Naming convention
    └── Checkout/
        └── CompleteEcommerceCheckout.feature
```

---

## 🚫 Anti-Patterns to Avoid

### ❌ Direct Playwright in Steps
```csharp
// BAD - Step definition
[When("I click the button")]
public async Task WhenIClickButton()
{
    await _session.Page.ClickAsync("button");  // ❌ WRONG!
}
```

### ✅ Correct Approach
```csharp
// GOOD - Step definition
[When("I click the button")]
public async Task WhenIClickButton()
{
    await RequireButtonPage().ClickSubmitButtonAsync();  // ✅ CORRECT!
}

// GOOD - Page Object
public async Task ClickSubmitButtonAsync()
{
    await page.ClickAsync("button");  // ✅ OK in Page Object
}
```

---

## 🎯 Benefits

1. **Maintainability**: Change selector in one place
2. **Reusability**: Page Objects used across multiple tests
3. **Readability**: Descriptive method names improve test clarity
4. **Testability**: Page Objects can be unit tested independently
5. **Scalability**: Easy to add new pages/features
6. **Separation of Concerns**: Test logic separate from UI interaction

---

## 📚 References

- See `REFACTORING_COMPLETE.md` for successful refactoring example
- See `tests/UI.Tests/PageObjects/` for reference implementations
- See `tests/UI.Tests/Steps/Checkout/` for proper step definitions

---

## 🚨 Enforcement

**When AI generates or modifies UI tests:**

1. ✅ **ALWAYS** create/use Page Objects
2. ✅ **NEVER** put Playwright calls in step definitions (except initial `GotoAsync`)
3. ✅ **REVIEW** existing code to ensure compliance
4. ✅ **REFACTOR** any violations immediately

**This is a NON-NEGOTIABLE framework standard.**

---

**Last Updated**: 2026-09-08  
**Status**: ✅ **MANDATORY RULE**
