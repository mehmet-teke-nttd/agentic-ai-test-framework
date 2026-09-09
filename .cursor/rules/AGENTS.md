# AI Assistant Guidelines for Agentic AI Test Framework

## 🎯 Mission

This document provides mandatory guidelines for AI assistants (Cursor, GitHub Copilot, etc.) working on this test framework.

---

## 🚨 CRITICAL RULES

### 0. **Naming Convention (MANDATORY)**

**All related files MUST use the same base name:**

```
{FeatureName}.feature          ← Feature file
{FeatureName}Steps.cs          ← Step definitions  
{FeatureName}Page.cs           ← Page Object
```

**Example - DemoButtonInteraction:**
```
✅ tests/UI.Tests/Features/Demo/DemoButtonInteraction.feature
✅ tests/UI.Tests/Steps/Demo/DemoButtonInteractionSteps.cs
   └── public sealed class DemoButtonInteractionSteps
✅ tests/UI.Tests/PageObjects/DemoButtonInteractionPage.cs
   └── public sealed class DemoButtonInteractionPage
```

**Example - UserProfile:**
```
✅ tests/UI.Tests/Features/UserProfile.feature
✅ tests/UI.Tests/Steps/UserProfileSteps.cs
   └── public sealed class UserProfileSteps
✅ tests/UI.Tests/PageObjects/UserProfilePage.cs
   └── public sealed class UserProfilePage
```

**❌ WRONG - Name Mismatch:**
```
❌ DemoButtonInteraction.feature
❌ DemoButtonInteractionSteps.cs
❌ ButtonTestingPage.cs  ← WRONG! Should be DemoButtonInteractionPage.cs
```

### 1. **Page Object Model Pattern (MANDATORY)**

**This framework STRICTLY follows the Page Object Model (POM) pattern.**

✅ **ALWAYS** create Page Objects in `tests/UI.Tests/PageObjects/`  
✅ **NEVER** put direct Playwright calls in step definitions  
✅ **READ** `.cursor/rules/RULE.md` before creating/modifying UI tests

**Reference**: [`.cursor/rules/RULE.md`](.cursor/rules/RULE.md)

### 2. **Test Layer Separation**

Tests are organized by layer:
- **UI Layer** (`tests/UI.Tests/`) - ReqnRoll + Playwright + Page Objects
- **API Layer** (`tests/API.Tests/`) - ReqnRoll + HttpClient
- **Integration Layer** (`tests/Integration.Tests/`) - Database + Service integration

**Reference**: [`docs/test-layers-guide.md`](docs/test-layers-guide.md)

### 3. **Framework Architecture**

- ✅ Gateway pattern for controlled test execution
- ✅ Evidence capture on failures
- ✅ LLM-powered failure analysis
- ✅ JSON-lines protocol for agent communication

**Reference**: [`docs/architecture.md`](docs/architecture.md)

---

## 📁 File Organization

### UI Tests Structure
```
tests/UI.Tests/
├── PageObjects/                    # ✅ ALL UI interactions here
│   ├── [Feature]Page.cs           # Encapsulate page logic
│   └── DemoButtonInteractionPage.cs
├── Steps/                         # ✅ Test orchestration only
│   └── Demo/
│       └── DemoButtonInteractionSteps.cs
├── Features/                      # Gherkin scenarios
│   └── Demo/
│       └── DemoButtonInteraction.feature
└── appsettings.json              # Application URLs
```

### Good Example (Use as Reference)
- ✅ `tests/UI.Tests/Features/Demo/DemoButtonInteraction.feature`
- ✅ `tests/UI.Tests/Steps/Demo/DemoButtonInteractionSteps.cs`
- ✅ `tests/UI.Tests/PageObjects/DemoButtonInteractionPage.cs`

This example demonstrates proper POM pattern with:
  - All navigation logic in Page Object (`NavigateToHomepageAsync`, `NavigateToButtonTestingPageAsync`)
  - All interactions in Page Object (click, menu selection, confirmation)
  - Step definitions orchestrating test flow only
  - Consistent naming across all three files

### Documentation
- 📚 `REFACTORING_COMPLETE.md` - Example of proper POM refactoring
- 📚 `docs/test-layers-guide.md` - Complete testing guide
- 📚 `docs/llm-failure-analysis-guide.md` - AI analysis features

---

## ✍️ Code Generation Guidelines

### When Creating UI Tests

1. **Choose Consistent Names**
   ```
   Feature: "DemoButtonInteraction"
   →  DemoButtonInteraction.feature
   →  DemoButtonInteractionSteps.cs
   →  DemoButtonInteractionPage.cs
   ```

2. **Create Page Object First**
   ```csharp
   // tests/UI.Tests/PageObjects/DemoButtonInteractionPage.cs
   public sealed class DemoButtonInteractionPage(IPage page)
   {
       private const string ButtonSelector = "button[type='submit']";
       
       public async Task ClickSubmitAsync()
       {
           await page.ClickAsync(ButtonSelector);
       }
   }
   ```

3. **Create Step Definitions Second**
   ```csharp
   // tests/UI.Tests/Steps/Demo/DemoButtonInteractionSteps.cs
   [Binding]
   public sealed class DemoButtonInteractionSteps(ScenarioContext scenarioContext)
   {
       private DemoButtonInteractionPage? _demoButtonInteractionPage;
       
       [When("I submit the form")]
       public async Task WhenISubmitForm()
       {
           await RequireDemoButtonInteractionPage().ClickSubmitAsync(); // ✅ Use Page Object
       }
       
       private DemoButtonInteractionPage RequireDemoButtonInteractionPage() =>
           _demoButtonInteractionPage ?? throw new InvalidOperationException("DemoButtonInteractionPage was not initialized.");
   }
   ```

4. **Create Feature File Third**
   ```gherkin
   # tests/UI.Tests/Features/Demo/DemoButtonInteraction.feature
   @Category:Smoke @Feature:DemoButtonInteraction @Risk:Medium @Layer:Ui
   Feature: Demo Button Interaction
     
     Scenario: Test scenario
       Given I am on the page
       When I submit the form
       Then I should see confirmation
   ```

### When Creating API Tests

- Use HttpClient via step definitions
- No Page Objects needed for API layer
- Follow ReqnRoll + HttpClient pattern
- Store configuration in `appsettings.json`

### When Creating Integration Tests

- Test database operations and service integration
- No UI/Page Objects
- Focus on data integrity and service contracts

---

## 🔍 Code Review Checklist

Before generating or modifying code, verify:

### UI Tests
- [ ] **Naming Convention**: Feature, Steps, and Page Object have same base name
  - [ ] `{FeatureName}.feature`
  - [ ] `{FeatureName}Steps.cs` with `class {FeatureName}Steps`
  - [ ] `{FeatureName}Page.cs` with `class {FeatureName}Page`
- [ ] Page Object exists in `tests/UI.Tests/PageObjects/`
- [ ] All selectors are constants in Page Object
- [ ] Step definitions use `using UI.Tests.PageObjects;`
- [ ] NO `page.Click()`, `page.Fill()`, etc. in step definitions
- [ ] Page Object initialized: `_pageObject = new {FeatureName}Page(RequireSession().Page);`
- [ ] Assertions in step definitions, not Page Objects
- [ ] Evidence capture in `[AfterScenario]`

### All Tests
- [ ] Correct layer tag: `@Layer:Ui`, `@Layer:Api`, or `@Layer:Integration`
- [ ] Category tag: `@Category:Smoke` or `@Category:Regression`
- [ ] Risk tag: `@Risk:Low/Medium/High/Critical`
- [ ] Feature tag: `@Feature:FeatureName`
- [ ] Proper async/await patterns
- [ ] Using statements for disposable resources

---

## 🎓 Learning Resources

### Read Before Working On:

| Task | Read These First |
|------|-----------------|
| **UI Tests** | `.cursor/rules/RULE.md`, `REFACTORING_COMPLETE.md` |
| **API Tests** | `docs/test-layers-guide.md`, `tests/API.Tests/` examples |
| **Integration Tests** | `docs/test-layers-guide.md`, `tests/Integration.Tests/` examples |
| **Failure Analysis** | `docs/llm-failure-analysis-guide.md` |
| **Architecture** | `docs/architecture.md`, `docs/agent-protocol.md` |
| **User Stories to Tests** | `.cursor/skills/tests-from-user-story/SKILL.md` |

---

## 🚀 Skills Available

The framework includes Cursor skills for automated test generation:

1. **`tests-from-user-story`** - Convert user stories to ReqnRoll features
2. **`expand-test-idea`** - Expand test ideas into comprehensive scenarios
3. **`ui-tests-from-screenshot`** - Generate tests from UI screenshots

**Location**: `.cursor/skills/`

---

## 🤖 Cursor SubAgents Integration

The framework uses **Cursor SubAgents** for development workflow automation.

### Built-in SubAgents (Cursor IDE)

These come with Cursor and work on ALL projects:

1. **`explore`** - Codebase search and analysis
   - Find test patterns and examples across entire codebase
   - Generate framework health reports
   - Example: "Map all UI tests and check POM compliance"

2. **`bash`** - Shell command execution
   - Run series of shell commands
   - Isolate command output from main context
   - Example: "Run dotnet build and capture output"

3. **`browser`** - Browser automation via MCP
   - Control browser for testing/automation
   - Navigate and interact with web pages
   - Example: "Test the application UI flow"

### Custom SubAgents (This Framework)

Located in `.cursor/agents/` - specific to THIS project:

1. **`pom-enforcer`** - Page Object Model enforcement
   - Catch direct Playwright calls in step definitions
   - Verify naming convention compliance
   - Validate POM pattern adherence
   - Example: "/pom-enforcer review my UI test changes"

2. **`naming-validator`** - Naming convention validation (Rule #0)
   - Check {Feature}.feature → {Feature}Steps.cs → {Feature}Page.cs
   - Verify class names match file names
   - Ensure consistency across test files
   - Example: "/naming-validator check Login test files"

3. **`test-architect`** - Test design and layer selection
   - Guide which layer to use (UI/API/Integration)
   - Verify metadata tags (@Category, @Risk, @Layer)
   - Plan test coverage and structure
   - Example: "/test-architect design checkout flow tests"

4. **`gateway-tester`** - Test.AgentGateway protocol validation
   - Test gateway JSON-lines protocol
   - Validate tool responses
   - Run tests via gateway and analyze results
   - Example: "/gateway-tester run smoke tests via gateway"

5. **`security-auditor`** - Security vulnerability scanning
   - Find hardcoded credentials and API keys
   - Check for sensitive test data
   - Validate environment variable usage
   - Example: "/security-auditor scan for exposed secrets"

**Total Available:** 3 built-in + 5 custom = **8 SubAgents**

### How to Use SubAgents

**Automatic Invocation:**
AI assistant will use SubAgents automatically based on task and description.

**Explicit Invocation:**
```
"/pom-enforcer review my UI test changes"
"/naming-validator check Login test files"
"/test-architect plan checkout tests"
"/gateway-tester run smoke tests"
"/security-auditor scan for secrets"
```

**Natural Language:**
```
"Use explore to find all smoke tests"
"Have pom-enforcer check my code"
"Ask test-architect which layer to use"
```

### When to Use SubAgents

#### ✅ Use SubAgents For:
- **Before coding:** `explore` to find patterns and examples
- **During development:** Generate tests with framework skills
- **Before commit:** `pom-enforcer` and `naming-validator` for quality checks
- **Test execution:** `gateway-tester` to run via Test.AgentGateway
- **Security:** `security-auditor` for credential/secret scanning
- **CI failures:** Built-in `bash` for command debugging
- **Framework audits:** `explore` for comprehensive health checks

#### ❌ Don't Use SubAgents For:
- **Test execution in production:** Use Test.AgentGateway CLI directly
- **Runtime failure analysis:** Use Test.AgentGateway LLM analyzer
- **Simple single-line edits:** Edit directly (faster)
- **Trivial checks:** Use IDE features instead

### SubAgent + Gateway Workflow

```
1. explore (built-in) → Find patterns
2. Skills → Generate tests
3. naming-validator (custom) → Check naming
4. pom-enforcer (custom) → Verify POM compliance
5. Gateway → Execute tests
6. Gateway LLM → Analyze failures
7. security-auditor (custom) → Security scan
```

### SubAgent Definitions

Custom SubAgents are defined in `.cursor/agents/` as markdown files with YAML frontmatter:

```markdown
---
name: my-subagent
description: When to use this SubAgent
model: inherit
readonly: true
---

Your instructions here...
```

**Documentation:**
- [Custom SubAgents README](../.cursor/agents/README.md) - How to use/create custom SubAgents
- [SubAgent Integration Guide](../docs/subagent-integration-guide.md) - Complete integration guide
- [SubAgent Workflows](../docs/subagent-workflows.md) - Practical workflows
- [SubAgent Quick Reference](../docs/SUBAGENT_QUICKREF.md) - Fast command lookup
- [Official Cursor Docs](https://cursor.com/docs/subagents) - Cursor SubAgent documentation

---

## ⚠️ Common Mistakes to Avoid

### ❌ DON'T
```csharp
// Step definition
[When("I click button")]
public async Task WhenIClick()
{
    await _session.Page.ClickAsync("button"); // ❌ Direct Playwright call
}
```

### ✅ DO
```csharp
// Step definition
[When("I click button")]
public async Task WhenIClick()
{
    await RequireButtonPage().ClickAsync(); // ✅ Use Page Object
}

// Page Object
public async Task ClickAsync()
{
    await page.ClickAsync("button"); // ✅ OK in Page Object
}
```

---

## 🏆 Quality Standards

This framework maintains high-quality standards:
- ✅ Zero code smells in refactored areas
- ✅ Production-ready code
- ✅ Industry-leading practices
- ✅ World-class architecture

**Framework Maturity Score: 9.8/10**

---

## 📞 Getting Help

When stuck:
1. Read `.cursor/rules/RULE.md` for Page Object Model guidance
2. Review `REFACTORING_COMPLETE.md` for refactoring examples
3. Check `docs/test-layers-guide.md` for layer-specific patterns
4. Look at existing code in `tests/UI.Tests/Steps/Demo/DemoButtonInteractionSteps.cs` as reference

---

## 📝 Change Protocol

When making changes:
1. **Understand the pattern** - Read relevant documentation first
2. **Follow existing examples** - Use refactored Checkout steps as reference
3. **Maintain consistency** - Match the established architecture
4. **Verify build** - Run `dotnet build` to ensure no errors
5. **Run tests** - Verify changes don't break existing tests

---

**Last Updated**: 2026-09-08  
**Status**: ✅ **ACTIVE GUIDELINES**
