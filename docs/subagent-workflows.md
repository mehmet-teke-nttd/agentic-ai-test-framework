# Cursor SubAgent Workflows

**Practical workflows combining SubAgents with Test.AgentGateway**  
**Last Updated:** 2026-09-09

---

## 🎯 Quick Start

### Workflow 1: Writing a New UI Test (15 minutes)

#### Step-by-Step:

1. **Explore existing patterns**
   ```
   💬 You: "Show me the DemoButtonInteraction test as an example"
   ```
   - Launch: `explore` SubAgent
   - Returns: Feature file, Steps, and Page Object structure
   - Study: Naming convention, POM pattern

2. **Generate your test** (using framework skills)
   ```
   💬 You: "Generate a login test from this user story: [paste story]"
   ```
   - Uses: `tests-from-user-story` skill
   - Creates: Login.feature, LoginSteps.cs, LoginPage.cs
   - Applies: Framework conventions automatically

3. **Pre-commit review**
   ```
   💬 You: "Review my login test with Bugbot"
   ```
   - Launch: `bugbot` SubAgent
   - Checks: POM compliance, naming convention, code quality
   - Reports: Any violations with line numbers

4. **Build & verify**
   ```powershell
   dotnet build tests/UI.Tests/UI.Tests.csproj
   ```
   - Verifies: Compilation successful
   - Fixes: Any build errors

5. **Run via Gateway**
   ```powershell
   dotnet run --project agent/Test.AgentGateway.Cli
   ```
   ```json
   {"tool":"run_tests","arguments":{"projectId":"ui-tests","testIds":["Login.ValidCredentials"]}}
   ```

6. **Commit**
   ```powershell
   git add tests/UI.Tests/Features/Login.feature
   git add tests/UI.Tests/Steps/LoginSteps.cs
   git add tests/UI.Tests/PageObjects/LoginPage.cs
   git commit -m "feat: Add login test with valid credentials"
   ```

**Time saved:** ~30 minutes vs manual writing

---

## 🔧 Workflow 2: Fixing POM Violations (10 minutes)

### Scenario: Direct Playwright calls found in step definitions

#### Step-by-Step:

1. **Discover violations**
   ```
   💬 You: "Find all direct Playwright calls in step definitions"
   ```
   - Launch: `explore` SubAgent
   - Returns: List of files with violations
   - Example: `CheckoutSteps.cs` has 3 `page.Click()` calls

2. **Review specific file**
   ```
   💬 You: "Show me CheckoutSteps.cs line 45"
   ```
   - Identifies: `await page.ClickAsync("#submit-button")`
   - Context: Inside `WhenISubmitTheOrder()` method

3. **Create Page Object method**
   ```csharp
   // CheckoutPage.cs
   public async Task SubmitOrderAsync()
   {
       await _page.ClickAsync("#submit-button");
       await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
   }
   ```

4. **Update step definition**
   ```csharp
   // CheckoutSteps.cs
   [When("I submit the order")]
   public async Task WhenISubmitTheOrder()
   {
       await RequireCheckoutPage().SubmitOrderAsync(); // ✅ Fixed
   }
   ```

5. **Verify with Bugbot**
   ```
   💬 You: "Review my CheckoutSteps.cs changes"
   ```
   - Launch: `bugbot` SubAgent
   - Reports: ✅ No violations found

6. **Build & test**
   ```powershell
   dotnet build
   ```

**Time saved:** ~20 minutes finding and fixing violations

---

## 🐛 Workflow 3: Debugging CI Failures (5 minutes)

### Scenario: PR tests pass locally but fail in GitHub Actions

#### Step-by-Step:

1. **Initial investigation**
   ```
   💬 You: "Investigate the failed UI test check in my PR"
   ```
   - Launch: `ci-investigator` SubAgent
   - Analyzes: GitHub Actions logs
   - Identifies: "Chromium executable not found"

2. **Get failure details via Gateway**
   ```json
   {"tool":"get_test_result","arguments":{"runId":"20260909T140000Z-..."}}
   ```
   - Returns: Full test result with evidence paths
   - Evidence: Screenshot not captured (browser didn't start)

3. **Analyze failure classification**
   ```json
   {"tool":"analyze_test_failure","arguments":{"runId":"20260909T140000Z-...","testId":"Login.ValidCredentials"}}
   ```
   - Returns: `Classification: Environment`, `Confidence: 0.95`
   - Reason: "Browser installation missing"

4. **Combined diagnosis**
   - **CI Investigator:** GitHub Action missing Playwright install step
   - **Gateway Analysis:** Environment classification (correct)
   - **Root cause:** CI pipeline configuration issue

5. **Fix workflow**
   ```yaml
   # .github/workflows/test.yml
   - name: Install Playwright
     run: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
   ```

6. **Rerun & verify**
   - Push workflow change
   - Tests now pass ✅

**Time saved:** ~45 minutes debugging environmental issues

---

## 📦 Workflow 4: Bulk Test Generation (30 minutes)

### Scenario: Generate API tests for all User Management endpoints

#### Step-by-Step:

1. **Explore API structure**
   ```
   💬 You: "Map all User Management API endpoints"
   ```
   - Launch: `explore` SubAgent
   - Returns: List of endpoints, HTTP methods, expected responses

2. **Generate tests in parallel**
   ```
   💬 You: "Generate API tests for:
   1. GET /users
   2. GET /users/{id}
   3. POST /users
   4. PUT /users/{id}
   5. DELETE /users/{id}"
   ```
   - Uses: `expand-test-idea` skill
   - Creates: Feature files with positive/negative/edge scenarios
   - Generates: Step definitions with HttpClient calls

3. **Review all generated tests**
   ```
   💬 You: "Review all UserManagement API test changes"
   ```
   - Launch: `bugbot` SubAgent
   - Checks: All tests follow framework conventions
   - Validates: Proper error handling, metadata tags

4. **Run smoke tests**
   ```json
   {"tool":"run_tests","arguments":{"projectId":"api-tests","categories":["Smoke"],"timeout":"00:05:00"}}
   ```

5. **Analyze any failures**
   ```json
   {"tool":"analyze_test_failure","arguments":{"runId":"...","testId":"UserAPI.GetUserById"}}
   ```

6. **Commit test suite**
   ```powershell
   git add tests/API.Tests/Features/UserManagement*.feature
   git add tests/API.Tests/Steps/UserManagement*.cs
   git commit -m "feat: Add comprehensive User Management API tests"
   ```

**Time saved:** ~2 hours vs manual test writing

---

## 🔄 Workflow 5: Framework Refactoring (1 hour)

### Scenario: Standardize all tests to use new Page Object navigation pattern

#### Step-by-Step:

1. **Map current state**
   ```
   💬 You: "Find all Page Objects and their navigation methods"
   ```
   - Launch: `explore` SubAgent (thorough mode)
   - Returns: Inventory of all Page Objects
   - Identifies: Inconsistent navigation patterns

2. **Define new pattern**
   ```csharp
   // Standard navigation pattern
   public async Task NavigateAsync(string url)
   {
       await _page.GotoAsync(url);
       await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
   }
   ```

3. **Refactor in batches**
   ```
   💬 You: "Update all Page Objects to use standard NavigateAsync pattern"
   ```
   - Launch: `generalPurpose` SubAgent
   - Updates: All Page Objects
   - Maintains: Existing functionality

4. **Verify with explore**
   ```
   💬 You: "Verify all Page Objects now have NavigateAsync method"
   ```
   - Launch: `explore` SubAgent
   - Reports: Compliance status
   - Identifies: Any missed files

5. **Full review**
   ```
   💬 You: "Run Bugbot review on all changed files"
   ```
   - Launch: `bugbot` SubAgent
   - Validates: Refactoring didn't break patterns
   - Checks: Build still succeeds

6. **Comprehensive testing**
   ```json
   {"tool":"run_tests","arguments":{"projectId":"ui-tests"}}
   ```

**Time saved:** ~4 hours vs manual refactoring + testing

---

## 🎯 Workflow 6: Test Maintenance Audit (20 minutes)

### Scenario: Monthly framework health check

#### Step-by-Step:

1. **Run comprehensive exploration**
   ```
   💬 You: "Generate complete framework health report"
   ```
   - Launch: `explore` SubAgent (very thorough)
   - Reports:
     - Test counts by layer
     - POM compliance percentage
     - Naming convention adherence
     - Tag coverage (metadata)
     - Framework health score

2. **Identify flaky tests**
   ```json
   {"tool":"get_failure_history","arguments":{"testId":"Login.ValidCredentials"}}
   ```
   - Gateway returns: Historical failures
   
   ```json
   {"tool":"analyze_failure_patterns","arguments":{"testId":"Login.ValidCredentials"}}
   ```
   - Gateway analyzes: Flakiness indicators
   - Identifies: Tests failing intermittently

3. **Security audit**
   ```
   💬 You: "Run security review on test configuration and API tests"
   ```
   - Launch: `security-review` SubAgent
   - Checks: No exposed credentials
   - Validates: Secure test data handling

4. **Generate action items**
   - Based on exploration report
   - Based on flaky test analysis
   - Based on security findings

5. **Create tracking issues**
   ```markdown
   ## Framework Health Issues
   
   1. **POM Violations** (Priority: High)
      - 2 files with direct Playwright calls
      - Files: CheckoutSteps.cs, PaymentSteps.cs
   
   2. **Flaky Tests** (Priority: Medium)
      - Login.ValidCredentials fails 30% of time
      - Likely: Timing issue with API response
   
   3. **Security** (Priority: Critical)
      - Hardcoded API key in ApiTests.cs
      - Action: Move to environment variable
   ```

**Time saved:** ~1 hour vs manual audit

---

## 🔗 Workflow 7: Integrating New Team Member (1 day onboarding)

### Day 1: Framework Familiarization

#### Morning: Learn the Structure

1. **Framework overview**
   ```
   💬 You: "Explain the test framework architecture and key components"
   ```
   - Launch: `explore` SubAgent
   - Returns: Architectural overview
   - Shows: Gateway, test layers, POM pattern

2. **Example deep-dive**
   ```
   💬 You: "Show me the DemoButtonInteraction test in detail"
   ```
   - Explores: Feature, Steps, Page Object
   - Explains: How they work together

3. **Pattern learning**
   ```
   💬 You: "What are the critical rules I must follow for UI tests?"
   ```
   - References: .cursor/rules/AGENTS.md
   - Highlights: POM pattern, naming convention

#### Afternoon: Hands-On Practice

4. **Create first test**
   ```
   💬 You: "Help me create a simple 'view profile' UI test"
   ```
   - Guided generation
   - Explains each component

5. **Review own work**
   ```
   💬 You: "Review my ViewProfile test"
   ```
   - Launch: `bugbot` SubAgent
   - Learn from feedback

6. **Run the test**
   ```json
   {"tool":"run_tests","arguments":{"projectId":"ui-tests","testIds":["ViewProfile.DisplaysUserName"]}}
   ```

**Result:** New developer productive in 1 day instead of 1 week

---

## 📋 Workflow Cheat Sheet

| Task | SubAgent | Typical Time | Command Template |
|------|----------|--------------|------------------|
| Find test examples | `explore` | 30 sec | "Show me tests matching [pattern]" |
| Pre-commit review | `bugbot` | 1 min | "Review my changes in tests/" |
| Security check | `security-review` | 2 min | "Security review on [files]" |
| Debug CI failure | `ci-investigator` | 3 min | "Investigate failed [check name]" |
| Generate test | Skills + `generalPurpose` | 5 min | Use test generation skills |
| Refactor pattern | `generalPurpose` | 15 min | "Update all [files] to use [pattern]" |
| Health audit | `explore` (thorough) | 5 min | "Generate framework health report" |

---

## 🎓 Pro Tips

### Tip 1: Scope Your Searches
```
❌ "Find all tests"  (too broad, slow)
✅ "Find all smoke tests in UI layer"  (scoped, fast)
```

### Tip 2: Use Specific Language
```
❌ "Check my code"  (vague)
✅ "Check for POM violations in CheckoutSteps.cs"  (specific)
```

### Tip 3: Combine Tools
```
1. explore → Find patterns
2. Skills → Generate code
3. bugbot → Validate
4. Gateway → Execute & analyze
```

### Tip 4: Automate Repetitive Tasks
Create hooks for:
- Pre-commit reviews
- Scheduled audits
- CI failure notifications

---

## 📚 Next Steps

1. **Try a workflow** - Pick one from above and execute it
2. **Customize hooks** - Edit `.cursor/hooks/hooks.json` for your team
3. **Share patterns** - Document successful workflows for your team
4. **Measure impact** - Track time saved using SubAgents

---

**Remember:** SubAgents accelerate development, Test.AgentGateway ensures quality. Use both! 🚀
