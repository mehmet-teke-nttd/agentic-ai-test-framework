# User Stories — DemoApps UI Testing Demo

Target application: [demoapps.qspiders.com](https://demoapps.qspiders.com/)

This document defines user stories and acceptance criteria for the UI testing demo. Each story maps to a feature area on the DemoApps practice site and will be converted into ReqnRoll test cases using the `tests-from-user-story` skill.

---

## Website Feature Catalog

| Feature | URL | Key Elements | Demo Priority |
|---------|-----|--------------|---------------|
| Text Field | `/ui` | Name, email, password inputs; placeholders; default values | High |
| Check Box | `/ui/checkbox` | Notification platforms, product recommendations, customer assistance | High |
| Dropdown | `/ui/dropdown` | Single select, multi-select, country/state/city cascading | High |
| Form Validation | `/ui/formValidation` | Required fields, validation messages, submit behavior | High |
| Radio Button | `/ui/radio` | Payment method, delivery options | Medium |
| Slider | `/ui/slider` | Price range slider | Medium |
| Toggle | `/ui/toggle` | On/off switches | Medium |
| Web Table | `/ui/table` | Sortable/filterable data grid | Low |
| Captcha | `/ui/captcha` | CAPTCHA verification | Low |

---

## US-DEMO-001: Text Field Registration

**Story ID:** US-DEMO-001  
**Requirement ID:** REQ-DEMO-TEXT-001  
**Feature:** TextFieldRegistration  
**Risk:** High

### User Story

As a **new user**  
I want to **enter my registration details into text fields on the registration form**  
So that **I can create an account and proceed with the application**

### Acceptance Criteria

| AC ID | Given | When | Then |
|-------|-------|------|------|
| AC-001-01 | I am on the registration page | I enter valid name, email, and password | The entered values should be visible in the respective text fields |
| AC-001-02 | I am on the registration page | I view the text fields | Each field should display its placeholder text |
| AC-001-03 | I am on the registration page with a field that has a default value | I view the default value field | The field should contain the pre-populated default value |
| AC-001-04 | I am on the registration page | I enter data into a text field and read it back | The captured value should match what I entered |
| AC-001-05 | I am on the registration page | I leave required fields empty and attempt to submit | Validation should prevent submission or show an error |

---

## US-DEMO-002: Multi-Select Dropdown Product Selection

**Story ID:** US-DEMO-002  
**Requirement ID:** REQ-DEMO-DROP-001  
**Feature:** DropdownProductSelection  
**Risk:** High

### User Story

As a **shopper**  
I want to **select products from multi-select and cascading dropdown menus**  
So that **I can configure my order with the correct product and location options**

### Acceptance Criteria

| AC ID | Given | When | Then |
|-------|-------|------|------|
| AC-002-01 | I am on the multi-select dropdown page | I select a single product from the product dropdown | The selected product should appear in the chosen products list |
| AC-002-02 | I am on the multi-select dropdown page | I select multiple products from the product dropdown | All selected products should appear in the chosen products list |
| AC-002-03 | I am on the multi-select dropdown page | I select a country using visible text | The country dropdown should reflect the selected country |
| AC-002-04 | I am on the multi-select dropdown page with a country selected | I select a state using value attribute | The state dropdown should reflect the selected state |
| AC-002-05 | I am on the multi-select dropdown page with state selected | I select a city using index | The city dropdown should reflect the selected city |
| AC-002-06 | I am on the multi-select dropdown page with valid selections | I click the Continue button | The page should proceed without errors |

---

## US-DEMO-003: Checkbox Notification Preferences

**Story ID:** US-DEMO-003  
**Requirement ID:** REQ-DEMO-CHK-001  
**Feature:** CheckboxPreferences  
**Risk:** Medium

### User Story

As a **customer completing an order**  
I want to **select my notification and product preference checkboxes**  
So that **I receive updates on my preferred platforms and product recommendations**

### Acceptance Criteria

| AC ID | Given | When | Then |
|-------|-------|------|------|
| AC-003-01 | I am on the checkbox preferences page | I select Email notification checkbox | The Email checkbox should be checked |
| AC-003-02 | I am on the checkbox preferences page | I select WhatsApp and Message notification checkboxes | Both checkboxes should be checked |
| AC-003-03 | I am on the checkbox preferences page | I select Shoes product recommendation checkbox | The Shoes checkbox should be checked |
| AC-003-04 | I am on the checkbox preferences page | I select all notification and recommendation checkboxes | All checkboxes should be checked |
| AC-003-05 | I am on the checkbox preferences page with all checkboxes selected | I click Continue | The page should proceed without errors |
| AC-003-06 | I am on the checkbox preferences page | No checkboxes are selected initially | All checkboxes should be unchecked by default |

---

## US-DEMO-004: Registration Form Validation

**Story ID:** US-DEMO-004  
**Requirement ID:** REQ-DEMO-FORM-001  
**Feature:** FormValidation  
**Risk:** Critical

### User Story

As a **system administrator**  
I want to **ensure the registration form validates user input correctly**  
So that **only valid data is accepted and users receive clear feedback on errors**

### Acceptance Criteria

| AC ID | Given | When | Then |
|-------|-------|------|------|
| AC-004-01 | I am on the registration page | I submit the form with all valid fields filled | The form should accept the submission |
| AC-004-02 | I am on the registration page | I submit with an empty name field | A validation error should be displayed for the name field |
| AC-004-03 | I am on the registration page | I submit with an invalid email format | A validation error should be displayed for the email field |
| AC-004-04 | I am on the registration page | I submit with an empty password field | A validation error should be displayed for the password field |
| AC-004-05 | I am on the registration page | I submit with all fields empty | Validation errors should be displayed and the form should not submit |

---

## Traceability Matrix

| User Story | Requirement ID | Feature File | Step Definitions |
|------------|----------------|--------------|------------------|
| US-DEMO-001 | REQ-DEMO-TEXT-001 | DemoTextFields.feature | DemoTextFieldsSteps.cs |
| US-DEMO-002 | REQ-DEMO-DROP-001 | DemoDropdowns.feature | DemoDropdownsSteps.cs |
| US-DEMO-003 | REQ-DEMO-CHK-001 | DemoCheckboxes.feature | DemoCheckboxesSteps.cs |
| US-DEMO-004 | REQ-DEMO-FORM-001 | DemoFormValidation.feature | DemoFormValidationSteps.cs |
