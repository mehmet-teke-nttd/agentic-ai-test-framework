---
name: git-commit
description: >-
  Create well-formatted git commits with conventional commit messages, proper descriptions,
  and automatic staging. Analyzes staged changes to generate descriptive commit messages
  following team conventions. Use when committing code, creating commits, writing commit
  messages, or when the user asks to "commit" or "save changes to git".
disable-model-invocation: true
---

# Git Commit Helper

This skill helps create professional, well-formatted git commits with conventional commit message patterns.

## Quick Start

When the user wants to commit changes:

1. **Check status** — Run `git status` to see what's changed
2. **Stage files** — Stage specific files or all changes
3. **Analyze changes** — Run `git diff --staged` to understand what's being committed
4. **Generate message** — Create a conventional commit message
5. **Commit** — Execute `git commit` with the generated message

---

## Conventional Commit Format

Use this structure for all commit messages:

```
<type>(<scope>): <short description>

<body>

<footer>
```

### Type Values

| Type | When to Use | Example |
|------|-------------|---------|
| `feat` | New feature or capability | `feat(ui): add button interaction test` |
| `fix` | Bug fix | `fix(api): correct null reference in auth handler` |
| `refactor` | Code restructuring (no behavior change) | `refactor(tests): extract common step definitions` |
| `test` | Adding or updating tests | `test(integration): add database connection tests` |
| `docs` | Documentation only | `docs(readme): update setup instructions` |
| `chore` | Build, config, dependencies | `chore(deps): update Playwright to 1.40.0` |
| `style` | Formatting, whitespace | `style(core): fix indentation in services` |
| `perf` | Performance improvements | `perf(db): optimize query execution` |
| `ci` | CI/CD changes | `ci(azure): add parallel test execution` |

### Scope Guidelines

**For this test framework, common scopes:**
- `ui` — UI tests, page objects, Playwright
- `api` — API tests, HTTP clients
- `integration` — Integration tests, database
- `core` — TestFramework.Core changes
- `gateway` — Agent gateway protocol
- `docs` — Documentation
- `skills` — Cursor skills
- `agents` — Subagent definitions

### Short Description Rules

- ✅ Start with lowercase verb: "add", "fix", "update", "remove"
- ✅ Keep under 72 characters
- ✅ No period at the end
- ✅ Be specific and descriptive
- ❌ Avoid vague messages like "update tests" or "fix bugs"

**Examples:**
```
✅ feat(ui): add registration form validation tests
✅ fix(gateway): handle null evidence in failure analysis
✅ refactor(core): extract common browser session logic
❌ update tests
❌ Fix stuff.
❌ WIP
```

### Body Guidelines (Optional)

Include a body when:
- The change needs explanation beyond the summary
- Multiple files/areas are affected
- Behavior changes need clarification
- Breaking changes require documentation

**Format:**
- Wrap at 72 characters per line
- Use bullet points for multiple items
- Explain **what** and **why**, not how (code shows how)

**Example:**
```
feat(ui): add comprehensive dropdown test scenarios

- Add single-select dropdown tests with all selection methods
- Add multi-select dropdown validation
- Create reusable DropdownPage object following POM pattern

Covers AC-002-01 through AC-002-08 from US-DEMO-002
```

### Footer (Optional)

Use footer for:
- **Breaking changes**: `BREAKING CHANGE: description`
- **Issue references**: `Closes #123`, `Fixes #456`, `Refs #789`
- **Requirement traceability**: `Implements REQ-UI-001`

---

## Workflow

### 1. Check Current Status

```bash
git status
```

Review:
- Untracked files (new files)
- Modified files
- Deleted files
- Currently staged files

### 2. Stage Changes

**Stage specific files:**
```bash
git add path/to/file1.cs path/to/file2.feature
```

**Stage all changes in a directory:**
```bash
git add tests/UI.Tests/Features/
```

**Stage all changes:**
```bash
git add -A
```

**Interactive staging (pick hunks):**
```bash
git add -p
```

### 3. Review Staged Changes

```bash
git diff --staged
```

Analyze:
- What files changed
- What functionality was added/modified/removed
- What scope this affects
- What type of change this represents

### 4. Generate Commit Message

**Single-file change:**
```
fix(ui): correct button selector in DemoButtonInteractionPage
```

**Multiple related changes:**
```
feat(ui): implement complete registration flow tests

- Add RegistrationPage object with form field methods
- Create RegistrationSteps with validation logic
- Add Registration.feature with smoke and regression scenarios

Follows POM pattern and naming conventions per AGENTS.md
```

**Breaking change:**
```
refactor(core)!: change evidence capture API to async

BREAKING CHANGE: CaptureEvidence() is now CaptureEvidenceAsync()
All step definitions must be updated to use async/await pattern.

Migrates evidence capture to async to support concurrent test execution.
```

### 5. Commit

```bash
git commit -m "type(scope): description"
```

**With body:**
```bash
git commit -m "type(scope): description" -m "
- First detail point
- Second detail point
- Third detail point
"
```

**Or use editor:**
```bash
git commit
# Opens default editor for multi-line message
```

---

## Special Cases

### Staging Patterns

**Stage new test files together:**
```bash
# Stage related test artifacts
git add tests/UI.Tests/Features/Demo/DemoButtonInteraction.feature
git add tests/UI.Tests/Steps/Demo/DemoButtonInteractionSteps.cs
git add tests/UI.Tests/PageObjects/DemoButtonInteractionPage.cs

# Commit as a unit
git commit -m "feat(ui): add button interaction test suite

Complete test implementation following POM pattern:
- Feature file with smoke and regression scenarios
- Step definitions with browser session handling
- Page object with button selectors and methods
"
```

**Amend last commit (add forgotten files):**
```bash
git add forgotten-file.cs
git commit --amend --no-edit
```

**Amend commit message:**
```bash
git commit --amend -m "corrected message"
```

### Multiple Logical Changes

If `git status` shows unrelated changes, commit them separately:

```bash
# First commit: UI test changes
git add tests/UI.Tests/
git commit -m "feat(ui): add dropdown selection tests"

# Second commit: Documentation
git add docs/
git commit -m "docs(testing): update POM guidelines"

# Third commit: Config changes
git add .cursor/rules/AGENTS.md
git commit -m "chore(rules): update naming convention examples"
```

### WIP Commits (Discouraged)

Avoid WIP commits in shared branches. If necessary:

```bash
# Temporary WIP commit
git commit -m "wip: registration tests in progress"

# Later, squash before pushing
git rebase -i HEAD~3
# Mark WIP commits as "fixup" or "squash"
```

---

## Framework-Specific Guidelines

### Test Commits

When committing test code for this framework:

**New test suite:**
```
feat(ui): add {FeatureName} test implementation

- Create {FeatureName}.feature with scenarios
- Implement {FeatureName}Steps with session handling
- Add {FeatureName}Page following POM pattern

Covers {requirement or user story reference}
```

**Refactoring existing tests:**
```
refactor(ui): migrate {Feature} to POM pattern

- Extract page logic to {FeatureName}Page
- Remove direct Playwright calls from steps
- Add reusable methods for common actions

No behavior changes; improves maintainability
```

**Test fixes:**
```
fix(ui): correct selector in {FeatureName}Page

Selector changed after UI update; test was failing on element not found
```

### Naming Convention Compliance

Always mention compliance with naming conventions:

```
feat(ui): add payment selection test suite

Complete implementation following AGENTS.md naming convention:
- PaymentSelection.feature
- PaymentSelectionSteps.cs
- PaymentSelectionPage.cs

All files use matching base name per framework standards
```

---

## Common Mistakes to Avoid

❌ **Vague messages**
```
git commit -m "update"
git commit -m "fixes"
git commit -m "WIP"
```

❌ **Missing type/scope**
```
git commit -m "add new tests"
```

❌ **Too generic**
```
git commit -m "feat: update tests"
```

❌ **Multiple unrelated changes in one commit**
```
# DON'T mix UI tests + documentation + config changes
```

✅ **Good examples**
```
git commit -m "feat(ui): add button interaction test suite"
git commit -m "fix(gateway): handle empty evidence directory"
git commit -m "docs(architecture): add subagent workflow diagrams"
git commit -m "chore(deps): update NUnit to 4.1.0"
```

---

## Integration with Other Skills

| Skill | Use Together |
|-------|--------------|
| `git-push` | After committing, use to push with validations |
| `git-checkpoint` | Quick alternative for WIP commits |

---

## Checklist

Before committing, verify:

- [ ] Changes are staged (`git status` shows green files)
- [ ] Commit message follows conventional format
- [ ] Type and scope are accurate
- [ ] Description is clear and under 72 chars
- [ ] Related files are committed together (feature + steps + page object)
- [ ] No unrelated changes are included
- [ ] Sensitive data (passwords, keys) is not committed

---

## When to Use This Skill

✅ Creating a commit  
✅ User asks to "commit changes"  
✅ User asks for help with commit messages  
✅ Reviewing staged changes before commit  
✅ Need to ensure conventional commit format  

## When NOT to Use

❌ Just staging files (use git add directly)  
❌ Pushing to remote (use `git-push` skill)  
❌ Quick WIP checkpoint (use `git-checkpoint` skill)  
