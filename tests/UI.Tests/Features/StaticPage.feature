@Category:Smoke @Feature:StaticPage @Risk:Low @Layer:Ui @Requirement:REQ-P0-001
Feature: Deterministic static page
  The browser scenario opens the configured application URL.
  Its default remains an in-memory data URL so the test is self-contained.

  Scenario: Show the local greeting
    Given a Chromium browser is available
    When I open the configured application
    Then the greeting is "Agentic test framework"
