# Cursor SubAgent Quick Reference

**📋 Fast lookup for SubAgent commands and workflows**

---

## 🚀 Quick Commands

### Explore Agent
```
"Show me the test framework structure"
"Find all smoke tests"
"Map all Page Objects and their methods"
"What's the framework health score?"
"Find tests matching [pattern]"
"Verify POM compliance for UI tests"
```

### Bugbot Review
```
"Review my uncommitted changes"
"Review changes in tests/UI.Tests/"
"Check for POM violations"
"Review my branch changes"
```

### Security Review
```
"Security review on API tests"
"Check for exposed credentials"
"Review authentication test code"
```

### CI Investigator
```
"Why did the UI test check fail?"
"Investigate failed PR check"
"Analyze CI pipeline failure"
```

### General Purpose
```
"Generate login tests for all scenarios"
"Refactor all Page Objects to use [pattern]"
"Create comprehensive API test suite"
```

---

## ⚡ Common Workflows

### Before Committing
1. `"Review my changes with Bugbot"`
2. Fix any violations
3. `dotnet build`
4. Commit

### Creating New Test
1. `"Show me existing [similar] test"`
2. Generate using skills
3. `"Review with Bugbot"`
4. Run via Gateway
5. Commit

### CI Failure
1. `"Investigate failed [check]"`
2. Gateway: `get_test_result`
3. Gateway: `analyze_test_failure`
4. Fix based on combined analysis
5. Rerun

### Monthly Audit
1. `"Generate framework health report"`
2. Gateway: `analyze_failure_patterns` for flaky tests
3. `"Security review on test configs"`
4. Create action items

---

## 🎯 SubAgent Selection Guide

| Need | Use This SubAgent | Example |
|------|------------------|---------|
| Find code/patterns | `explore` | "Find all API tests" |
| Pre-commit check | `bugbot` | "Review my changes" |
| Security audit | `security-review` | "Check for secrets" |
| CI debugging | `ci-investigator` | "Why did CI fail?" |
| Generate multiple tests | `generalPurpose` | "Create test suite" |
| Understand structure | `explore` | "Explain architecture" |
| Enforce rules | `bugbot` | "Check POM compliance" |

---

## 🔗 Integration with Test.AgentGateway

| Phase | Tool | Purpose |
|-------|------|---------|
| **Design** | SubAgent (`explore`) | Find patterns, examples |
| **Generate** | Framework Skills | Create Feature/Steps/Page |
| **Review** | SubAgent (`bugbot`) | Validate compliance |
| **Execute** | Test.AgentGateway | Run tests |
| **Analyze** | Test.AgentGateway (LLM) | Classify failures |
| **Debug** | SubAgent (`ci-investigator`) | CI/pipeline issues |

---

## 📁 Framework Files to Know

| File | Purpose | When to Read |
|------|---------|--------------|
| `.cursor/rules/AGENTS.md` | Mandatory AI guidelines | Before any test work |
| `.cursor/rules/RULE.md` | POM implementation rules | Before UI tests |
| `.cursor/hooks/hooks.json` | Automated workflows | Setup automation |
| `docs/subagent-integration-guide.md` | Full SubAgent guide | Deep dive |
| `docs/subagent-workflows.md` | Practical workflows | Learning by example |

---

## ✅ Pre-Commit Checklist

- [ ] Run: `"Review my changes with Bugbot"`
- [ ] Fix: Any reported violations
- [ ] Build: `dotnet build`
- [ ] Verify: No warnings/errors
- [ ] Commit

---

## 🐛 Troubleshooting

### "SubAgent didn't find violations"
→ Check `.cursor/rules/AGENTS.md` is up to date

### "Exploration is slow"
→ Scope search: "Find X in tests/UI.Tests/" not "Find X"

### "CI Investigator can't read logs"
→ Setup `gh` CLI or copy logs locally

### "Bugbot approved but build fails"
→ Run `dotnet build` locally, SubAgent checks code quality not compilation

---

## 🎓 Learning Path

**Day 1: Exploration**
```
"Show me test framework structure"
"Explain Page Object Model pattern"
"Show me a complete test example"
```

**Day 2: Creation**
```
"Generate a simple test"
"Review my test with Bugbot"
"Run test via Gateway"
```

**Day 3: Mastery**
```
"Generate test suite for [feature]"
"Refactor tests to new pattern"
"Set up automated pre-commit reviews"
```

---

## 📊 Success Metrics

Track these to measure effectiveness:

- **POM Violations:** Should trend to 0
- **Review Time:** Bugbot < 1 min vs manual 15+ min
- **CI Debug Time:** 5 min vs 45 min without SubAgents
- **Test Generation:** 10x faster with SubAgent + Skills

---

## 🔧 Automated Hooks

Location: `.cursor/hooks/hooks.json`

Available hooks:
- `pre-commit-pom-review` - Auto-review before commit
- `explore-test-coverage` - Generate test inventory
- `verify-framework-compliance` - Full compliance check
- `find-test-examples` - Pattern-based example finder

Run hooks manually or configure triggers.

---

## 💡 Pro Tips

1. **Be Specific:** "Find POM violations in CheckoutSteps.cs" > "Check my code"
2. **Scope Searches:** Use file/directory paths for faster results
3. **Combine Tools:** Explore → Generate → Review → Execute → Analyze
4. **Automate Repetitive:** Create hooks for routine checks
5. **Track Impact:** Measure time saved with vs without SubAgents

---

## 🆘 Getting Help

1. **Full Guide:** `docs/subagent-integration-guide.md`
2. **Workflows:** `docs/subagent-workflows.md`
3. **Framework Rules:** `.cursor/rules/AGENTS.md`
4. **Gateway Protocol:** `docs/agent-protocol.md`

---

## 🎯 Your First SubAgent Command

Try this now:
```
"Explore the test framework and tell me the current health score"
```

Then:
```
"Review my last changes with Bugbot"
```

**You're ready to go! 🚀**
