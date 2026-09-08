@Category:Smoke @Feature:EcommerceCheckout @Risk:Critical @Layer:Ui @Requirement:REQ-CHECKOUT-001
Feature: Complete E-commerce Checkout Flow
  Users should be able to complete the entire checkout process from registration through order confirmation
  The flow includes user registration, payment method selection, notification preferences, privacy settings, and final confirmation
  
  Background:
    Given a Chromium browser is available
    And I have selected items to purchase

  # ============================================================================
  # HAPPY PATH: Complete end-to-end checkout flow
  # ============================================================================
  
  @Category:Smoke @Risk:Critical
  Scenario: Complete successful checkout from registration to confirmation
    # Screen 1: User Registration
    Given I am on the registration page
    When I enter name "John Doe"
    And I enter email "john.doe@example.com"
    And I enter password "SecurePass123!"
    And I click the "Register" button
    
    # Screen 2: Payment Method Selection
    Then I should be on the payment selection page
    And I should see the order summary with "Shoes" quantity "1" total "100"
    When I select "UPI" as payment method
    And I select "Office delivery" as delivery option with time "09 AM - 06 PM"
    And I click "Continue"
    
    # Screen 3: Notification Preferences
    Then I should be on the notification preferences page
    And the order summary should still show total "100"
    When I check "Email" notification
    And I check "WhatsApp" notification
    And I check "Sandals" product recommendations
    And I click "Continue"
    
    # Screen 4: Privacy Settings
    Then I should be on the privacy settings page
    When I toggle on "profile visibility to delivery partner"
    And I toggle on "name visibility to delivery partners"
    And I click "Place Order"
    
    # Screen 5: Order Confirmation
    Then I should see the order confirmation page
    And I should see "ORDER PLACED" message
    And I should see "Your Order has been successfully placed!" message
    And I should see the order confirmation image

  # ============================================================================
  # ALTERNATIVE PATHS: Different selections across the flow
  # ============================================================================

  @Category:Smoke @Risk:High
  Scenario: Complete checkout with Credit Card and Home Delivery
    Given I complete registration with valid credentials
    When I am on the payment selection page
    And I select "Credit/Debit/ATM card" as payment method
    And I select "Home delivery" as delivery option with time "08 AM - 09 PM"
    And I proceed through notification preferences with "Message" selected
    And I proceed through privacy settings with all toggles OFF
    Then the order should be placed successfully
    And the confirmation should reflect "Credit/Debit/ATM card" payment
    And the confirmation should reflect "Home delivery" option

  @Category:Smoke @Risk:High
  Scenario: Complete checkout with Cash on Delivery and minimal notifications
    Given I complete registration with valid credentials
    When I am on the payment selection page
    And I select "Cash on Delivery" as payment method
    And I select "Office delivery" as delivery option
    And I proceed without selecting any notifications
    And I proceed through privacy settings with all toggles OFF
    Then the order should be placed successfully

  # ============================================================================
  # DATA PERSISTENCE: Information carries across screens
  # ============================================================================

  @Category:Regression @Risk:High
  Scenario: User information persists throughout checkout flow
    Given I enter "Jane Smith" as my name during registration
    And I enter "jane.smith@example.com" as my email
    And I complete registration
    When I navigate through all checkout screens
    Then my name "Jane Smith" should be visible in the order confirmation
    And my email should be reflected in notification settings
    And my selections should be preserved

  @Category:Regression @Risk:High
  Scenario: Order summary persists across all checkout screens
    Given I have "Shoes" in my cart with quantity "1" and total "100"
    When I complete registration
    Then the order summary should show "Shoes" with total "100"
    When I select payment method
    Then the order summary should remain unchanged
    When I select notification preferences
    Then the order summary should still show total "100"
    When I reach privacy settings
    Then the order summary should still be visible with correct total

  # ============================================================================
  # NAVIGATION: Back and forth between screens
  # ============================================================================

  @Category:Regression @Risk:High
  Scenario: Navigate back from payment selection to registration
    Given I am on the registration page
    When I complete registration form
    And I am on the payment selection page
    And I click the browser back button
    Then I should return to the registration page
    And my registration data should be preserved
    And I can modify my information
    And proceed forward again

  @Category:Regression @Risk:Medium
  Scenario: Navigate back from privacy settings to notifications
    Given I have progressed to the privacy settings page
    When I click back to notification preferences
    Then I should see my previously selected notifications
    And I can modify my notification choices
    And proceed forward with updated preferences

  # ============================================================================
  # VALIDATION: Prevent progression with incomplete data
  # ============================================================================

  @Category:Regression @Risk:High
  Scenario: Cannot proceed without completing registration
    Given I am on the registration page
    When I leave the email field empty
    And I attempt to register
    Then I should see a validation error for email
    And I should remain on the registration page
    And cannot access payment selection

  @Category:Regression @Risk:High
  Scenario: Cannot proceed without selecting payment method
    Given I complete registration successfully
    And I am on the payment selection page
    When I do not select any payment method
    And I attempt to continue
    Then I should see a validation error
    And the continue button should be disabled
    And I cannot proceed to notification preferences

  @Category:Regression @Risk:Medium
  Scenario: Registration validation prevents bad data from entering flow
    Given I am on the registration page
    When I enter invalid email "not-an-email"
    And I enter weak password "123"
    And I attempt to register
    Then I should see multiple validation errors
    And the registration should not proceed
    And no data should enter the checkout flow

  # ============================================================================
  # PAYMENT METHOD VARIATIONS
  # ============================================================================

  @Category:Regression @Risk:Critical
  Scenario Outline: Complete checkout with different payment methods
    Given I complete registration successfully
    When I am on the payment selection page
    And I select "<payment_method>" as payment method
    And I select "<delivery_option>" as delivery option
    And I complete the remaining checkout steps
    Then the order confirmation should show "<payment_method>"
    And the confirmation should show "<delivery_option>"

    Examples:
      | payment_method           | delivery_option  |
      | UPI                      | Office delivery  |
      | Credit/Debit/ATM card    | Home delivery    |
      | Net Banking              | Office delivery  |
      | EMI(Easy installments)   | Home delivery    |
      | Cash on Delivery         | Office delivery  |

  # ============================================================================
  # NOTIFICATION PREFERENCES VARIATIONS
  # ============================================================================

  @Category:Regression @Risk:Medium
  Scenario: Complete checkout with all notifications enabled
    Given I complete registration and payment selection
    When I am on the notification preferences page
    And I check "Email" notification
    And I check "WhatsApp" notification
    And I check "Message" notification
    And I check "Yahoo" notification
    And I check all product recommendation options
    And I complete checkout
    Then all notification preferences should be saved
    And I should receive notifications on all channels

  @Category:Regression @Risk:Medium
  Scenario: Complete checkout with no notifications selected
    Given I complete registration and payment selection
    When I am on the notification preferences page
    And I leave all notification checkboxes unchecked
    And I check "No thanks" for recommendations
    And I complete checkout
    Then the order should be placed successfully
    And no notification preferences should be saved

  # ============================================================================
  # PRIVACY SETTINGS VARIATIONS
  # ============================================================================

  @Category:Regression @Risk:Medium
  Scenario: Complete checkout with full privacy protection
    Given I complete registration, payment, and notifications
    When I am on the privacy settings page
    And I leave all privacy toggles OFF
    And I click "Place Order"
    Then the order should be placed successfully
    And my personal information should remain private
    And delivery partner should receive minimal information

  @Category:Regression @Risk:Medium
  Scenario: Complete checkout with all information shared
    Given I complete registration, payment, and notifications
    When I am on the privacy settings page
    And I toggle on all privacy settings
    And I click "Place Order"
    Then the order should be placed successfully
    And my profile should be visible to delivery partner
    And my name, phone, and email should be shared

  # ============================================================================
  # ERROR HANDLING
  # ============================================================================

  @Category:Regression @Risk:High
  Scenario: Handle registration failure gracefully
    Given I am on the registration page
    When I attempt to register with an already-used email "existing@example.com"
    Then I should see "Email already registered" error
    And I should remain on the registration page
    And I should not proceed to payment selection
    And I can correct my email and retry

  @Category:Regression @Risk:Medium
  Scenario: Handle payment selection timeout
    Given I complete registration successfully
    And I am on the payment selection page
    When I remain idle for extended period
    Then my session should remain valid
    And I should see a session warning
    And I can continue checkout after interaction

  # ============================================================================
  # SPECIAL CASES
  # ============================================================================

  @Category:Regression @Risk:Low
  Scenario: Skip optional sections where allowed
    Given I complete registration successfully
    When I quickly proceed through all required sections
    And I use minimal selections for optional items
    Then the checkout should complete successfully
    And only required information should be collected

  @Category:Regression @Risk:High
  Scenario: Prevent duplicate order placement
    Given I have completed checkout and received confirmation
    When I try to navigate back to payment selection
    Then I should not be able to place the same order again
    And I should see a warning that order is already placed
    And I should be directed to order confirmation or home page

  @Category:Regression @Risk:Medium
  Scenario: Delivery time reflects selected delivery option
    Given I complete registration successfully
    When I select "Office delivery" on payment page
    Then the delivery time should show "09 AM - 06 PM"
    When I change to "Home delivery"
    Then the delivery time should update to "08 AM - 09 PM"
    When I complete checkout
    Then the confirmation should reflect the selected delivery time

  # ============================================================================
  # ACCESSIBILITY & USABILITY
  # ============================================================================

  @Category:Regression @Risk:Low
  Scenario: Complete checkout using only keyboard navigation
    Given I am on the registration page
    When I use Tab to navigate through fields
    And I fill in all required information
    And I press Enter to submit registration
    And I navigate through payment selection using keyboard
    And I complete all sections using only keyboard
    Then the checkout should complete successfully
    And order confirmation should be displayed

  @Category:Regression @Risk:Low
  Scenario: Checkout flow is responsive on mobile viewport
    Given I access the checkout flow on a mobile device
    When I complete registration on small screen
    And I select payment method on mobile
    And I proceed through all checkout steps
    Then all screens should be properly formatted
    And all interactive elements should be accessible
    And the order should be placed successfully
