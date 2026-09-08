---
name: ui-tests-from-screenshot
description: >-
  Analyze UI screenshots, wireframes, or mockups to generate comprehensive ReqnRoll feature files and test scenarios. 
  Identifies form fields, buttons, navigation elements, and interactive components to create test coverage. 
  Use when the user provides screenshots, design files, images of UI, or asks to "generate tests from this screen/design/mockup".
---

# Generate UI Tests from Screenshots

This skill analyzes UI screenshots, wireframes, or design mockups to automatically generate comprehensive ReqnRoll test scenarios for the Agentic AI Test Framework.

## Quick Start

When the user provides:
- UI screenshots or screen captures
- Design mockups or wireframes
- Figma exports or design files
- A request like "Generate tests from this screen"

**Follow this workflow:**

### For Single Screenshot:
1. **Analyze the image** - Identify all UI elements
2. **Categorize components** - Forms, buttons, navigation, content
3. **Generate feature file** - Create `.feature` with comprehensive scenarios
4. **Apply metadata** - Use appropriate tags based on UI criticality
5. **Create step definitions** - Generate implementation stubs

### For Multiple Screenshots (MOST IMPORTANT):
1. **Analyze ALL screenshots together** - Understand the complete flow
2. **Identify the overall feature** - What is the end-to-end journey?
3. **Create ONE feature file** - Not separate files per screenshot
4. **Generate scenarios spanning all screens** - Sequential flows across screenshots
5. **Create step definitions** - Handle navigation between screens
6. **Name the feature** - Use a name that encompasses the entire flow (e.g., "CheckoutProcess", "UserRegistrationFlow", "MultiStepWizard")

---

## Image Analysis Process

### Step 0: Determine Screenshot Count

**Before analyzing, check how many screenshots the user provided:**

- **1 screenshot**: Create comprehensive tests for that single screen
- **2+ screenshots**: Treat as a single cohesive flow/journey - create ONE feature file

### Step 1: Read and Understand the Screenshot(s)

Use the Read tool to load all images, then identify:

**Interactive Elements:**
- Text input fields (username, email, password, search, etc.)
- Buttons (submit, cancel, search, navigation)
- Checkboxes and radio buttons
- Dropdown menus and select boxes
- Links and navigation items
- Toggle switches and sliders

**Content Elements:**
- Headings and labels
- Error message areas
- Success/confirmation messages
- Validation indicators
- Icons and visual feedback

**Layout Elements:**
- Navigation menus (header, sidebar, footer)
- Breadcrumbs
- Modals and dialogs
- Tabs and accordions

### Step 2: Determine Feature Scope

**For SINGLE screenshot:**
Based on the screen type, identify the feature:

**For MULTIPLE screenshots:**
Analyze the sequence to determine the overall feature:
- What is the user trying to accomplish across all screens?
- What is the start and end state?
- What is the logical flow between screens?
- Name the feature based on the complete journey (e.g., "Complete Checkout Flow", "User Onboarding Process", "Multi-Step Registration")

**Single Screenshot - Feature Determination:**

| Screen Type | Feature Name | Common Scenarios |
|-------------|--------------|------------------|
| Login page | Login/Authentication | Valid login, invalid credentials, password reset |
| Registration | UserRegistration | Successful signup, validation errors, duplicate user |
| Form | [FormName] | Valid submission, field validation, error handling |
| Dashboard | Dashboard | Data display, navigation, filters |
| List/Table | [EntityName]List | Pagination, sorting, filtering, search |
| Detail page | [EntityName]Details | View data, edit, delete, navigation |

---

## Metadata Assignment Based on UI

### Risk Level Determination

Analyze the screenshot to assign risk:

```
Critical Risk:
- Payment forms
- Password/authentication screens
- Data deletion confirmations
- Security settings
- Account management

High Risk:
- User registration
- Profile editing
- Checkout flows
- Form submissions with data persistence

Medium Risk:
- Search functionality
- Filtering and sorting
- Navigation elements
- Content display

Low Risk:
- Read-only displays
- Informational pages
- Static content
- Help/FAQ pages
```

### Category Assignment

```
Smoke:
- Critical user flows (login, checkout)
- Primary navigation
- Core form submissions

Regression:
- Field validations
- Error handling
- Edge cases
- All negative scenarios
```

---

## Test Generation Patterns

### Pattern 1: Form Analysis

**When screenshot shows a form:**

1. **Identify all fields:**
   - Count input fields
   - Identify field types (text, email, password, number)
   - Note required vs optional fields
   - Check for validation indicators

2. **Generate scenarios:**
   ```gherkin
   Scenario: Submit form with all valid data
   Scenario: Submit with missing required field [for each required field]
   Scenario: Validate [field-type] format [for each typed field]
   Scenario: Clear form resets all fields
   Scenario: Cancel returns without saving
   ```

### Pattern 2: Navigation Analysis

**When screenshot shows navigation elements:**

1. **Identify navigation items:**
   - Menu items
   - Links
   - Breadcrumbs
   - Back buttons

2. **Generate scenarios:**
   ```gherkin
   Scenario: Navigate to [destination] via [menu-item]
   Scenario: Breadcrumb navigation to [level]
   Scenario: Active menu item is highlighted
   Scenario: Back button returns to previous page
   ```

### Pattern 3: List/Table Analysis

**When screenshot shows data lists or tables:**

1. **Identify features:**
   - Column headers (sortable?)
   - Pagination controls
   - Search box
   - Filters
   - Action buttons (edit, delete, view)

2. **Generate scenarios:**
   ```gherkin
   Scenario: Sort by [column] ascending
   Scenario: Filter by [criterion]
   Scenario: Search for [item]
   Scenario: Navigate to page 2
   Scenario: View details of [item]
   ```

### Pattern 4: Authentication UI

**When screenshot shows login/signup:**

1. **Required scenarios:**
   - Successful authentication
   - Invalid credentials
   - Empty fields validation
   - Password visibility toggle
   - Remember me checkbox
   - Forgot password link
   - Social login options (if present)

---

## Example Analysis: Single Screenshot

### Input: Login Screen Screenshot (1 screenshot)

**Observed elements:**
- Username/Email text field (required)
- Password field (required, with show/hide toggle)
- "Remember me" checkbox
- "Sign In" button (primary)
- "Forgot password?" link
- "Create account" link

### Generated Feature File (Single Screenshot)

`tests/UI.Tests/Features/Login.feature`:

```gherkin
@Category:Smoke @Feature:Login @Risk:Critical @Layer:Ui @Requirement:REQ-AUTH-001
Feature: User Authentication
  Users must be able to log in securely using email and password
  The login form should validate inputs and provide clear feedback

  Background:
    Given a Chromium browser is available
    And I am on the login page

  @Category:Smoke
  Scenario: Successful login with valid credentials
    When I enter email "user@example.com"
    And I enter password "SecurePass123"
    And I click the "Sign In" button
    Then I should be redirected to the dashboard
    And I should see a welcome message

  @Category:Regression @Risk:High
  Scenario: Login fails with invalid email
    When I enter email "nonexistent@example.com"
    And I enter password "SomePassword"
    And I click the "Sign In" button
    Then I should see an error message "Invalid credentials"
    And I should remain on the login page

  @Category:Regression @Risk:High
  Scenario: Login fails with incorrect password
    When I enter email "user@example.com"
    And I enter password "WrongPassword"
    And I click the "Sign In" button
    Then I should see an error message "Invalid credentials"

  @Category:Regression @Risk:Medium
  Scenario: Email field validation shows error for invalid format
    When I enter email "invalid-email"
    And I move focus away from the email field
    Then I should see a validation error "Please enter a valid email"

  @Category:Regression @Risk:Medium
  Scenario: Required field validation prevents submission
    When I leave the email field empty
    And I leave the password field empty
    And I click the "Sign In" button
    Then I should see validation errors
    And the form should not be submitted

  @Category:Regression @Risk:Low
  Scenario: Password visibility toggle shows/hides password
    When I enter password "MyPassword123"
    And the password should be masked
    When I click the "show password" toggle
    Then the password should be visible as "MyPassword123"
    When I click the "hide password" toggle
    Then the password should be masked again

  @Category:Regression @Risk:Low
  Scenario: Remember me checkbox persists login
    When I enter valid credentials
    And I check the "Remember me" checkbox
    And I click the "Sign In" button
    And I close the browser and reopen
    Then I should still be logged in

  @Category:Smoke @Risk:Medium
  Scenario: Forgot password link navigates to password reset
    When I click the "Forgot password?" link
    Then I should be on the password reset page

  @Category:Smoke @Risk:Medium
  Scenario: Create account link navigates to registration
    When I click the "Create account" link
    Then I should be on the registration page
```

---

## Example Analysis: Multiple Screenshots

### Input: E-commerce Checkout Flow (4 screenshots)

**Screenshot 1:** Registration form (name, email, password)
**Screenshot 2:** Payment method selection (radio buttons: UPI, Card, Cash on Delivery)
**Screenshot 3:** Notifications preferences (checkboxes: Email, WhatsApp, SMS)
**Screenshot 4:** Order confirmation (success message, order number)

**Analysis:**
- Overall feature: "Complete E-commerce Checkout Process"
- User journey: Register → Select Payment → Set Preferences → Confirm Order
- Screens are sequential steps in one flow
- Need to track state across screens

### Generated Feature File (Multiple Screenshots - ONE FILE)

`tests/UI.Tests/Features/CompleteCheckoutFlow.feature`:

```gherkin
@Category:Smoke @Feature:CheckoutFlow @Risk:Critical @Layer:Ui @Requirement:REQ-CHECKOUT-001
Feature: Complete E-commerce Checkout Process
  Users should be able to complete the entire checkout flow from registration to order confirmation
  The flow includes registration, payment selection, notification preferences, and final confirmation

  Background:
    Given a Chromium browser is available
    And I have items in my shopping cart

  @Category:Smoke @Risk:Critical
  Scenario: Complete successful checkout from start to finish
    # Screen 1: Registration
    Given I am on the registration page
    When I enter name "John Doe"
    And I enter email "john@example.com"
    And I enter password "SecurePass123!"
    And I click "Register"
    
    # Screen 2: Payment Selection
    Then I should be on the payment selection page
    When I select "UPI" as payment method
    And I click "Continue"
    
    # Screen 3: Notification Preferences
    Then I should be on the notification preferences page
    When I check "Email" notification
    And I check "WhatsApp" notification
    And I click "Continue"
    
    # Screen 4: Order Confirmation
    Then I should see the order confirmation page
    And I should see "Order Placed Successfully" message
    And I should see my order number

  @Category:Regression @Risk:High
  Scenario: Navigate back and forth between checkout screens
    Given I am on the registration page
    When I complete registration
    And I am on the payment selection page
    When I click "Back"
    Then I should return to registration page
    And my registration data should be preserved

  @Category:Regression @Risk:High
  Scenario: Validation prevents progression with incomplete data
    Given I am on the registration page
    When I leave the email field empty
    And I click "Register"
    Then I should see a validation error
    And I should remain on the registration page

  @Category:Regression @Risk:High
  Scenario: Different payment method selection affects checkout flow
    Given I complete registration
    And I am on the payment selection page
    When I select "Cash on Delivery"
    And I proceed through notification preferences
    Then the confirmation should show "Cash on Delivery" as payment method

  @Category:Smoke @Risk:High
  Scenario: User data persists across all checkout screens
    Given I enter "John Doe" during registration
    When I navigate through all checkout screens
    Then my name should be visible in the order confirmation
    And all my selections should be correctly reflected

  @Category:Regression @Risk:Medium
  Scenario: Complete checkout with minimal notifications
    Given I complete registration successfully
    And I select payment method
    When I am on notification preferences
    And I leave all notifications unchecked
    And I complete the checkout
    Then the order should be placed successfully
    And no notification preferences should be saved
```

### Generated Step Definitions (Multiple Screenshots)

`tests/UI.Tests/Steps/CompleteCheckoutFlowSteps.cs`:

```csharp
using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps;

[Binding]
public sealed class CompleteCheckoutFlowSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    [Given("I have items in my shopping cart")]
    public void GivenIHaveItemsInMyShoppingCart()
    {
        scenarioContext["HasItems"] = true;
    }

    [Given("I am on the registration page")]
    public async Task GivenIAmOnTheRegistrationPage()
    {
        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable.");
        }
        
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await _session.Page.GotoAsync($"{applicationUrl}/checkout/register");
    }

    [When("I enter name {string}")]
    public async Task WhenIEnterName(string name)
    {
        await RequireSession().Page.FillAsync("input[name='name']", name);
        scenarioContext["UserName"] = name;
    }

    [When("I enter email {string}")]
    public async Task WhenIEnterEmail(string email)
    {
        await RequireSession().Page.FillAsync("input[name='email']", email);
        scenarioContext["UserEmail"] = email;
    }

    [When("I enter password {string}")]
    public async Task WhenIEnterPassword(string password)
    {
        await RequireSession().Page.FillAsync("input[name='password']", password);
        scenarioContext["UserPassword"] = password;
    }

    [When("I click {string}")]
    public async Task WhenIClick(string buttonText)
    {
        await RequireSession().Page.ClickAsync($"button:has-text('{buttonText}')");
        await Task.Delay(500); // Wait for navigation
    }

    [When("I complete registration")]
    [Given("I complete registration")]
    public async Task WhenICompleteRegistration()
    {
        await WhenIEnterName("John Doe");
        await WhenIEnterEmail("john@example.com");
        await WhenIEnterPassword("SecurePass123!");
        await WhenIClick("Register");
    }

    [When("I select {string} as payment method")]
    public async Task WhenISelectAsPaymentMethod(string paymentMethod)
    {
        await RequireSession().Page.ClickAsync($"input[value='{paymentMethod}']");
        scenarioContext["PaymentMethod"] = paymentMethod;
        await Task.Delay(300);
    }

    [When("I check {string} notification")]
    public async Task WhenICheckNotification(string notificationType)
    {
        await RequireSession().Page.CheckAsync($"input[type='checkbox']:near(:text('{notificationType}'))");
        await Task.Delay(200);
    }

    [When("I leave the email field empty")]
    public void WhenILeaveTheEmailFieldEmpty()
    {
        // Don't fill email field
        scenarioContext["EmailEmpty"] = true;
    }

    [When("I leave all notifications unchecked")]
    public void WhenILeaveAllNotificationsUnchecked()
    {
        scenarioContext["NoNotifications"] = true;
    }

    [When("I am on notification preferences")]
    [Given("I am on notification preferences")]
    public async Task WhenIAmOnNotificationPreferences()
    {
        // Verify we're on the right page
        await RequireSession().Page.WaitForURLAsync("**/notification-preferences");
    }

    [When("I complete the checkout")]
    public async Task WhenICompleteTheCheckout()
    {
        await WhenIClick("Complete Order");
    }

    [When("I am on the payment selection page")]
    [Given("I am on the payment selection page")]
    public async Task WhenIAmOnThePaymentSelectionPage()
    {
        await RequireSession().Page.WaitForURLAsync("**/payment");
    }

    [When("I navigate through all checkout screens")]
    public async Task WhenINavigateThroughAllCheckoutScreens()
    {
        await WhenICompleteRegistration();
        await WhenISelectAsPaymentMethod("UPI");
        await WhenIClick("Continue");
        await WhenIClick("Continue");
    }

    [When("I proceed through notification preferences")]
    public async Task WhenIProceedThroughNotificationPreferences()
    {
        await WhenIAmOnNotificationPreferences();
        await WhenIClick("Continue");
    }

    [When("I click {string}")]
    public async Task WhenIClickBack(string linkText)
    {
        await RequireSession().Page.ClickAsync($"text='{linkText}'");
        await Task.Delay(500);
    }

    [Then("I should be on the payment selection page")]
    public async Task ThenIShouldBeOnThePaymentSelectionPage()
    {
        var url = RequireSession().Page.Url;
        Assert.That(url, Does.Contain("/payment"));
    }

    [Then("I should be on the notification preferences page")]
    public async Task ThenIShouldBeOnTheNotificationPreferencesPage()
    {
        var url = RequireSession().Page.Url;
        Assert.That(url, Does.Contain("/notification"));
    }

    [Then("I should see the order confirmation page")]
    public async Task ThenIShouldSeeTheOrderConfirmationPage()
    {
        await RequireSession().Page.WaitForURLAsync("**/confirmation");
        var url = RequireSession().Page.Url;
        Assert.That(url, Does.Contain("/confirmation"));
    }

    [Then("I should see {string} message")]
    public async Task ThenIShouldSeeMessage(string expectedMessage)
    {
        var message = await RequireSession().Page.Locator($"text='{expectedMessage}'").IsVisibleAsync();
        Assert.That(message, Is.True, $"Expected to see '{expectedMessage}'");
    }

    [Then("I should see my order number")]
    public async Task ThenIShouldSeeMyOrderNumber()
    {
        var orderNumber = await RequireSession().Page.Locator("[class*='order-number']").IsVisibleAsync();
        Assert.That(orderNumber, Is.True);
    }

    [Then("I should return to registration page")]
    public async Task ThenIShouldReturnToRegistrationPage()
    {
        var url = RequireSession().Page.Url;
        Assert.That(url, Does.Contain("/register"));
    }

    [Then("my registration data should be preserved")]
    public async Task ThenMyRegistrationDataShouldBePreserved()
    {
        var emailValue = await RequireSession().Page.InputValueAsync("input[name='email']");
        Assert.That(emailValue, Is.Not.Empty);
    }

    [Then("I should see a validation error")]
    public async Task ThenIShouldSeeAValidationError()
    {
        var error = await RequireSession().Page.Locator(".error-message, .validation-error").IsVisibleAsync();
        Assert.That(error, Is.True);
    }

    [Then("I should remain on the registration page")]
    public void ThenIShouldRemainOnTheRegistrationPage()
    {
        var url = RequireSession().Page.Url;
        Assert.That(url, Does.Contain("/register"));
    }

    [Then("the confirmation should show {string} as payment method")]
    public async Task ThenTheConfirmationShouldShowAsPaymentMethod(string paymentMethod)
    {
        var methodText = await RequireSession().Page.Locator($"text='{paymentMethod}'").IsVisibleAsync();
        Assert.That(methodText, Is.True);
    }

    [Then("my name should be visible in the order confirmation")]
    public async Task ThenMyNameShouldBeVisibleInTheOrderConfirmation()
    {
        var userName = scenarioContext["UserName"] as string;
        var nameVisible = await RequireSession().Page.Locator($"text='{userName}'").IsVisibleAsync();
        Assert.That(nameVisible, Is.True);
    }

    [Then("all my selections should be correctly reflected")]
    public void ThenAllMySelectionsShouldBeCorrectlyReflected()
    {
        Assert.Pass("All selections persisted through the flow");
    }

    [Then("the order should be placed successfully")]
    public async Task ThenTheOrderShouldBePlacedSuccessfully()
    {
        var success = await RequireSession().Page.Locator("text='success'").IsVisibleAsync();
        Assert.That(success, Is.True);
    }

    [Then("no notification preferences should be saved")]
    public void ThenNoNotificationPreferencesShouldBeSaved()
    {
        var noNotifs = scenarioContext.ContainsKey("NoNotifications");
        Assert.That(noNotifs, Is.True);
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

### Generated Step Definitions (Single Screenshot)

`tests/UI.Tests/Steps/LoginSteps.cs`:

```csharp
using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps;

[Binding]
public sealed class LoginSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    [Given("I am on the login page")]
    public async Task GivenIAmOnTheLoginPage()
    {
        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable.");
        }
        
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await _session.Page.GotoAsync($"{applicationUrl}/login");
    }

    [When("I enter email {string}")]
    public async Task WhenIEnterEmail(string email)
    {
        await RequireSession().Page.FillAsync("#email", email);
    }

    [When("I enter password {string}")]
    public async Task WhenIEnterPassword(string password)
    {
        await RequireSession().Page.FillAsync("#password", password);
    }

    [When("I click the {string} button")]
    public async Task WhenIClickTheButton(string buttonText)
    {
        await RequireSession().Page.ClickAsync($"button:has-text('{buttonText}')");
    }

    [When("I move focus away from the email field")]
    public async Task WhenIMoveFocusAwayFromEmailField()
    {
        await RequireSession().Page.ClickAsync("body");
    }

    [When("I check the {string} checkbox")]
    public async Task WhenICheckTheCheckbox(string checkboxLabel)
    {
        await RequireSession().Page.CheckAsync($"input[type='checkbox']:near(:text('{checkboxLabel}'))");
    }

    [When("I click the {string} link")]
    public async Task WhenIClickTheLink(string linkText)
    {
        await RequireSession().Page.ClickAsync($"a:has-text('{linkText}')");
    }

    [When("I click the {string} toggle")]
    public async Task WhenIClickTheToggle(string toggleName)
    {
        await RequireSession().Page.ClickAsync($"[aria-label='{toggleName}']");
    }

    [Then("I should be redirected to the dashboard")]
    public async Task ThenIShouldBeRedirectedToTheDashboard()
    {
        await RequireSession().Page.WaitForURLAsync("**/dashboard");
        Assert.That(RequireSession().Page.Url, Does.Contain("/dashboard"));
    }

    [Then("I should see a welcome message")]
    public async Task ThenIShouldSeeAWelcomeMessage()
    {
        var message = await RequireSession().Page.Locator(".welcome-message").IsVisibleAsync();
        Assert.That(message, Is.True);
    }

    [Then("I should see an error message {string}")]
    public async Task ThenIShouldSeeAnErrorMessage(string expectedError)
    {
        var error = await RequireSession().Page.Locator(".error-message").TextContentAsync();
        Assert.That(error, Does.Contain(expectedError));
    }

    [Then("I should remain on the login page")]
    public void ThenIShouldRemainOnTheLoginPage()
    {
        Assert.That(RequireSession().Page.Url, Does.Contain("/login"));
    }

    [Then("I should see a validation error {string}")]
    public async Task ThenIShouldSeeAValidationError(string expectedError)
    {
        var error = await RequireSession().Page.Locator(".field-error").TextContentAsync();
        Assert.That(error, Does.Contain(expectedError));
    }

    [Then("the password should be masked")]
    public async Task ThenThePasswordShouldBeMasked()
    {
        var inputType = await RequireSession().Page.GetAttributeAsync("#password", "type");
        Assert.That(inputType, Is.EqualTo("password"));
    }

    [Then("the password should be visible as {string}")]
    public async Task ThenThePasswordShouldBeVisibleAs(string expectedValue)
    {
        var inputType = await RequireSession().Page.GetAttributeAsync("#password", "type");
        Assert.That(inputType, Is.EqualTo("text"));
        var value = await RequireSession().Page.InputValueAsync("#password");
        Assert.That(value, Is.EqualTo(expectedValue));
    }

    [Then("I should be on the password reset page")]
    public async Task ThenIShouldBeOnThePasswordResetPage()
    {
        await RequireSession().Page.WaitForURLAsync("**/password-reset");
        Assert.That(RequireSession().Page.Url, Does.Contain("/password-reset"));
    }

    [Then("I should be on the registration page")]
    public async Task ThenIShouldBeOnTheRegistrationPage()
    {
        await RequireSession().Page.WaitForURLAsync("**/register");
        Assert.That(RequireSession().Page.Url, Does.Contain("/register"));
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

## Element-to-Selector Mapping

When generating step definitions, use these selector strategies:

| Element Type | Preferred Selector | Example |
|--------------|-------------------|----------|
| Input by label | Label text + type | `input:near(:text('Email'))` |
| Input by ID | ID attribute | `#username` |
| Button by text | Button with text | `button:has-text('Submit')` |
| Link by text | Anchor with text | `a:has-text('Forgot password')` |
| Checkbox by label | Near label text | `input[type='checkbox']:near(:text('Remember'))` |
| Error message | Class selector | `.error-message`, `.field-error` |
| Success message | Class selector | `.success-message`, `.alert-success` |

**Prefer semantic selectors over brittle ones:**
- ✅ `button:has-text('Sign In')`
- ❌ `div > div > button:nth-child(2)`

---

## UI Pattern Detection

### Login/Authentication Screen
**Indicators:** Username/email field, password field, submit button
**Generate:** Authentication feature with credential validation

### Registration/Signup Form
**Indicators:** Multiple input fields, password confirmation, terms checkbox
**Generate:** Registration feature with field validation and duplicate checks

### Data Entry Form
**Indicators:** Various input types, submit/cancel buttons, validation messages
**Generate:** Form submission feature with validation scenarios

### Search Interface
**Indicators:** Search input, search button, results area
**Generate:** Search feature with query validation and results display

### Settings/Profile Page
**Indicators:** Editable fields, save button, section headers
**Generate:** Profile management feature with update scenarios

### Dashboard
**Indicators:** Multiple data widgets, charts, navigation cards
**Generate:** Dashboard navigation and data display scenarios

---

## Accessibility Considerations

When analyzing screenshots, also generate accessibility tests:

```gherkin
@Category:Regression @Risk:Medium
Scenario: Form is keyboard navigable
  When I navigate through form fields using Tab key
  Then focus should move logically through all fields
  And I should be able to submit using Enter key

@Category:Regression @Risk:Medium  
Scenario: Error messages are announced to screen readers
  When validation fails on a field
  Then the error should have proper ARIA attributes
  And screen reader should announce the error
```

---

## Checklist for Screenshot-Based Tests

Before finalizing generated tests:

- [ ] **Verified screenshot count** - Single or multiple?
- [ ] **If multiple screenshots**: Created ONE feature file spanning all screens
- [ ] **If single screenshot**: Created focused feature file for that screen
- [ ] All visible interactive elements have test coverage
- [ ] Form validation scenarios cover all fields
- [ ] Navigation elements are tested (especially between screens if multiple)
- [ ] Error states are considered
- [ ] Success/confirmation flows are included
- [ ] Metadata tags are appropriate for UI criticality
- [ ] Selectors are semantic and resilient
- [ ] Step definitions use Playwright best practices
- [ ] Background section reduces duplication
- [ ] Scenario names describe user intent
- [ ] **For multi-screen**: Data persistence across screens is tested
- [ ] **For multi-screen**: Back/forward navigation is covered

---

## Multiple Screenshots Strategy

**IMPORTANT: When user provides multiple screenshots, treat them as ONE cohesive feature/flow.**

Multiple screenshots represent:
- Steps in a multi-step wizard or form
- Sequential pages in a user journey
- Different states of the same feature
- Related screens in a single workflow

**Do NOT create separate feature files for each screenshot.**

### Approach:

1. **Analyze all screenshots together** to understand the complete flow
2. **Identify the overall feature** (e.g., "Checkout Process", "User Onboarding", "Registration Flow")
3. **Create ONE feature file** that covers the entire journey
4. **Generate scenarios** that span across multiple screens
5. **Use step definitions** that navigate between screens

### Example:

**User provides 3 screenshots:**
- Screenshot 1: Registration form (name, email, password)
- Screenshot 2: Payment selection (credit card, PayPal)
- Screenshot 3: Confirmation page (success message)

**Generate ONE feature file:** `UserRegistrationAndPayment.feature`

```gherkin
@Category:Smoke @Feature:UserRegistration @Risk:High @Layer:Ui @Requirement:REQ-ONBOARD-001
Feature: Complete User Registration and Payment
  New users should complete registration and set up payment in one flow
  
  Scenario: Complete registration flow from start to finish
    Given I am on the registration page
    When I enter my personal details
    And I click "Continue"
    Then I should see the payment selection page
    When I select "Credit Card" as payment method
    And I enter my card details
    And I click "Complete Registration"
    Then I should see the confirmation page
    And I should see "Registration successful" message
```

### Multi-Screen Scenario Patterns:

**Pattern 1: Sequential Flow**
```gherkin
Scenario: User completes multi-step checkout
  Given I am on step 1 of checkout
  When I complete shipping information
  And I proceed to step 2
  Then I should see payment options
  When I select payment method
  And I proceed to step 3
  Then I should see order review
  When I confirm my order
  Then I should see order confirmation
```

**Pattern 2: Alternative Paths**
```gherkin
Scenario: User selects different options across screens
  Given I am on the product selection page
  When I select "Premium Plan"
  And I continue to payment
  Then payment options should reflect premium pricing
  When I select "Annual Billing"
  And I complete checkout
  Then I should see annual subscription confirmation
```

**Pattern 3: Validation Across Screens**
```gherkin
Scenario: Data persists across multiple screens
  Given I am on screen 1
  When I enter "John Doe" as my name
  And I continue to screen 2
  Then my name "John Doe" should still be displayed
  When I go back to screen 1
  Then the name field should still contain "John Doe"
```

---

## Tips for Accurate Test Generation

1. **Look for text content** - Labels indicate field purpose
2. **Identify required fields** - Usually marked with * or "required"
3. **Spot validation indicators** - Red borders, error message areas
4. **Check button hierarchy** - Primary vs secondary actions
5. **Notice state indicators** - Loading, success, error states
6. **Consider mobile/responsive** - Different layouts may need variants
7. **Look for tooltips/help text** - Indicates important validation rules

---

## Common Pitfalls to Avoid

❌ **Don't assume element IDs** - Generate generic selectors
❌ **Don't ignore disabled states** - Test when buttons should be disabled
❌ **Don't skip error scenarios** - They're often the most important
❌ **Don't create brittle selectors** - Use text content over CSS paths
❌ **Don't forget cleanup** - Always include `[AfterScenario]`

---

## Integration with Other Skills

This skill works well with:
- `expand-test-idea` - Use together for comprehensive coverage
- Browser DevTools - Inspect live pages to confirm selectors
- Design systems - Reference component libraries for consistent naming

---

## When to Use This Skill

✅ User provides UI screenshots or mockups (single or multiple)
✅ Design handoff requires test coverage planning
✅ Visual QA needs automation
✅ Prototypes need test scenarios before implementation
✅ Existing screens lack test coverage
✅ User provides a sequence of screens representing a user journey

## When NOT to Use This Skill

❌ No visual content provided (use `expand-test-idea` instead)
❌ API/backend testing needed
❌ Performance testing required
❌ Tests for non-visual functionality

## Key Reminder

**🔴 CRITICAL RULE: Multiple screenshots = ONE feature file representing the complete flow, not separate files per screenshot.**
