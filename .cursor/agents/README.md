# Custom SubAgents for Agentic AI Test Framework

This folder contains **custom SubAgents** specific to this test framework.

---

## 🎯 What Are Custom SubAgents?

Custom SubAgents are specialized AI assistants that understand YOUR project's rules and patterns. When you or an AI assistant works on this codebase, these SubAgents can be invoked to perform specific tasks.

### Built-in SubAgents (Cursor IDE)
These come with Cursor and work on ALL projects:
- **explore** - Codebase search and analysis
- **bash** - Shell command execution
- **browser** - Browser automation

### Custom SubAgents (This Framework)
These are specific to THIS test framework and enforce OUR rules:
- **pom-enforcer** - Page Object Model pattern enforcement
- **naming-validator** - Naming convention validation
- **test-architect** - Test layer and structure guidance
- **gateway-tester** - Test.AgentGateway protocol validation
- **security-auditor** - Security vulnerability scanning

---

## 📋 Available Custom SubAgents

### 1. **pom-enforcer** 🛡️
**Purpose:** Enforce Page Object Model pattern in UI tests

**When to use:**
- Creating new UI tests
- Reviewing UI test changes
- Pre-commit checks for UI layer

**What it checks:**
- No direct Playwright calls in step definitions
- All interactions in Page Objects
- Proper naming: {Feature}.feature → {Feature}Steps.cs → {Feature}Page.cs
- Selectors as constants, not inline strings

**Example:**
```
"Review my UI test with pom-enforcer"
"/pom-enforcer check DemoButtonInteractionSteps.cs"
```

---

### 2. **naming-validator** ✍️
**Purpose:** Validate naming convention consistency (Rule #0)

**When to use:**
- Before creating new test files
- After renaming tests
- Pre-commit naming checks

**What it checks:**
- Feature file name matches Steps file name
- Steps file name matches Page Object name
- Class names match file names
- No abbreviations or variations

**Example:**
```
"Validate naming for Login test with naming-validator"
"/naming-validator check tests/UI.Tests/Features/Login.feature"
```

---

### 3. **test-architect** 🏗️
**Purpose:** Guide test layer selection and structure

**When to use:**
- Planning new test coverage
- Deciding which layer to test in
- Designing test structure

**What it checks:**
- Correct layer selection (UI/API/Integration)
- Proper metadata tags (@Category, @Risk, @Layer)
- Appropriate test patterns for each layer
- Coverage planning (positive, negative, edge cases)

**Example:**
```
"Which layer should I test user authentication in? Ask test-architect"
"/test-architect plan checkout flow tests"
```

---

### 4. **gateway-tester** 🔧
**Purpose:** Validate Test.AgentGateway protocol and execution

**When to use:**
- Testing gateway protocol changes
- Running tests via gateway
- Validating failure analysis

**What it checks:**
- JSON-lines protocol format
- Gateway tool responses
- Evidence capture completeness
- LLM analysis accuracy

**Example:**
```
"Test the gateway discover_tests tool with gateway-tester"
"/gateway-tester run smoke tests and analyze results"
```

---

### 5. **security-auditor** 🔐
**Purpose:** Scan for security vulnerabilities in test code

**When to use:**
- Before committing authentication tests
- Adding API integrations
- Security compliance reviews

**What it checks:**
- Hardcoded credentials
- API keys in code
- Connection strings with real passwords
- Sensitive test data (real PII)
- Environment variable usage

**Example:**
```
"Run security audit on API tests with security-auditor"
"/security-auditor check for exposed credentials"
```

---

## 🚀 How to Use

### Automatic Invocation
AI assistant will automatically use these SubAgents when appropriate based on their descriptions.

### Explicit Invocation
Use the `/name` syntax:
```
"/pom-enforcer review my UI test changes"
"/naming-validator check Login test files"
"/test-architect design checkout tests"
```

### Natural Language
Mention the SubAgent in your request:
```
"Use the pom-enforcer subagent to check my code"
"Ask test-architect which layer to use"
"Have security-auditor scan for secrets"
```

---

## 📁 File Format

Each SubAgent is a markdown file with YAML frontmatter:

```markdown
---
name: my-subagent
description: Short description that helps AI decide when to use this
model: inherit
readonly: true
---

Your instructions here...
```

### Configuration Fields

| Field | Description | Options |
|-------|-------------|---------|
| `name` | SubAgent identifier | lowercase-with-hyphens |
| `description` | When to use this SubAgent | Include "Use proactively" or "Use when..." |
| `model` | Which AI model to use | `inherit` (same as parent) or specific model |
| `readonly` | Can it modify files? | `true` (read-only) or `false` (can edit) |

---

## 🔄 Workflow Integration

### Pre-Commit Workflow
```
1. naming-validator → Check file names
2. pom-enforcer → Check POM compliance
3. security-auditor → Check for secrets
4. Commit if all pass ✅
```

### New Test Creation Workflow
```
1. test-architect → Choose layer and structure
2. Generate test files
3. naming-validator → Verify naming
4. pom-enforcer → Verify patterns (if UI)
5. gateway-tester → Run via gateway
```

### Security Review Workflow
```
1. security-auditor → Scan test code
2. Fix any Critical/High issues
3. Re-scan to verify fixes
4. Commit ✅
```

---

## 🎓 Best Practices

### ✅ DO:
- Use SubAgents for specialized validation
- Invoke before committing changes
- Combine SubAgents in workflows
- Update SubAgent prompts as patterns evolve

### ❌ DON'T:
- Use SubAgents for simple single-line checks
- Invoke all SubAgents unnecessarily
- Duplicate built-in SubAgent functionality
- Create overly generic SubAgents

---

## 📊 SubAgent Comparison

| SubAgent | Focus | Layer | When | Readonly |
|----------|-------|-------|------|----------|
| **pom-enforcer** | POM pattern | UI only | UI test review | ✅ Yes |
| **naming-validator** | Naming convention | All layers | Before commit | ✅ Yes |
| **test-architect** | Test design | All layers | Planning phase | ✅ Yes |
| **gateway-tester** | Gateway protocol | Framework | Testing gateway | ❌ No |
| **security-auditor** | Security | All layers | Auth/sensitive data | ✅ Yes |

---

## 🔧 Creating New SubAgents

To add a new custom SubAgent:

1. Create `.cursor/agents/{name}.md`
2. Add YAML frontmatter with name and description
3. Write clear, specific instructions
4. Test with explicit invocation
5. Refine based on results

**Example:**
```markdown
---
name: performance-checker
description: Performance test validator. Use when creating performance tests.
model: inherit
readonly: true
---

You validate performance test structure...
```

---

## 📚 Related Documentation

- [Official Cursor SubAgent Docs](https://cursor.com/docs/subagents)
- [SubAgent Integration Guide](../docs/subagent-integration-guide.md)
- [SubAgent Workflows](../docs/subagent-workflows.md)
- [Framework Rules](../.cursor/rules/AGENTS.md)

---

## 🎯 Quick Reference Commands

```bash
# Use pom-enforcer
"/pom-enforcer review my UI tests"

# Use naming-validator
"/naming-validator check Login test"

# Use test-architect
"/test-architect design checkout tests"

# Use gateway-tester
"/gateway-tester run smoke tests"

# Use security-auditor
"/security-auditor scan for secrets"
```

---

**Remember:** These SubAgents are tools to enforce YOUR framework's rules. They read from `.cursor/rules/` to understand the patterns and standards specific to this project.

---

**Status:** ✅ Production Ready  
**Last Updated:** 2026-09-09  
**SubAgent Count:** 5 custom + 3 built-in = 8 total
