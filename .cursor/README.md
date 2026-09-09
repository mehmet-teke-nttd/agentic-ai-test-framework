# .cursor Folder - README

**Purpose:** Configuration for AI assistants working on this test framework

---

## 🎯 What This Folder Contains

This folder configures how AI assistants (like Cursor AI, GitHub Copilot) work with your project. It does NOT define SubAgents - those are built into Cursor IDE.

---

## 📁 Folder Structure

```
.cursor/
├── rules/           ← AI Assistant Guidelines
├── skills/          ← Custom Code Generation
└── hooks/           ← Automation Workflows
```

---

## 📂 Folders Explained

### 1. `.cursor/rules/` - AI Assistant Guidelines

**Purpose:** Tell AI assistants the RULES of your project

**Files:**
- `AGENTS.md` - Mandatory guidelines for AI code generation
  - Naming conventions
  - Page Object Model pattern
  - Code quality standards
  - When to use SubAgents

- `RULE.md` - Page Object Model implementation rules
  - UI test patterns
  - Selector management
  - Step definition structure

**How it works:**
When an AI assistant (or SubAgent) works on your code, it reads these files to understand YOUR project's rules and patterns.

---

### 2. `.cursor/skills/` - Custom Code Generation

**Purpose:** Templates for generating code in YOUR framework's style

**Files:**
- `expand-test-idea/SKILL.md` - Expand test ideas into full scenarios
- `tests-from-user-story/SKILL.md` - Convert user stories to tests
- `ui-tests-from-screenshot/SKILL.md` - Generate tests from screenshots

**How it works:**
When you ask to generate code, skills provide templates that follow your framework patterns automatically.

---

### 3. `.cursor/hooks/` - Automation Workflows

**Purpose:** Trigger AI actions automatically

**Files:**
- `hooks.json` - Automated workflow configuration
  - Pre-commit reviews
  - Test exploration
  - Framework compliance checks

**How it works:**
Hooks can trigger AI actions at specific times (e.g., before commit, on schedule).

---

## 🤖 What About SubAgents?

**SubAgents are NOT defined here!**

SubAgents (`explore`, `bugbot`, `security-review`, `ci-investigator`, `generalPurpose`) are **built into Cursor IDE** - they're available to all Cursor projects.

### What IS Defined Here:

✅ **Project rules** that SubAgents will FOLLOW  
✅ **Code templates** that match YOUR framework  
✅ **Automation triggers** for WHEN to use SubAgents

### What is NOT Defined Here:

❌ SubAgent definitions (they're built into Cursor)  
❌ SubAgent capabilities (determined by Cursor)  
❌ SubAgent availability (they always exist)

---

## 🔄 How They Work Together

```
┌─────────────────────────────────────────────┐
│         Cursor IDE (Has SubAgents)          │
│  ┌─────────────────────────────────────┐   │
│  │  Built-in SubAgents:                │   │
│  │  • explore                          │   │
│  │  • bugbot                           │   │
│  │  • security-review                  │   │
│  │  • ci-investigator                  │   │
│  │  • generalPurpose                   │   │
│  └──────────────┬──────────────────────┘   │
└─────────────────┼───────────────────────────┘
                  │
                  │ Reads
                  ▼
┌─────────────────────────────────────────────┐
│    Your Project's .cursor/ Folder           │
│  ┌─────────────────────────────────────┐   │
│  │  rules/AGENTS.md                    │   │
│  │  - POM pattern requirements         │   │
│  │  - Naming conventions               │   │
│  │  - Code quality standards           │   │
│  └─────────────────────────────────────┘   │
│                                             │
│  SubAgent understands YOUR project rules   │
└─────────────────────────────────────────────┘
```

**Key Insight:** SubAgents are tools in Cursor, `.cursor/` folder teaches them about YOUR project.

---

## 📚 Key Files Purpose

| File | What It Does | Who Uses It |
|------|--------------|-------------|
| `rules/AGENTS.md` | Project-wide AI guidelines | All AI assistants + SubAgents |
| `rules/RULE.md` | UI test POM pattern | AI assistants generating UI tests |
| `skills/*/SKILL.md` | Code generation templates | AI when generating specific code |
| `hooks/hooks.json` | Automation configuration | Cursor IDE for triggering actions |

---

## ✅ Summary

**You DON'T define SubAgents here** - they're built into Cursor IDE.

**You DO define:**
1. How AI should write code for YOUR project (`rules/`)
2. Templates for generating YOUR style of code (`skills/`)
3. When to trigger AI actions (`hooks/`)

**Think of it like:**
- **Cursor** = Has the tools (SubAgents)
- **`.cursor/` folder** = Instructions for using those tools on YOUR project

---

## 🚀 Usage

### For Developers:
- Read `rules/AGENTS.md` before coding
- Follow patterns in existing code
- AI assistants will enforce these rules

### For AI Assistants:
- Read `rules/` to understand project conventions
- Use `skills/` for code generation
- Follow `hooks/` for automation triggers

### For SubAgents:
- Built into Cursor (no setup needed)
- Automatically read `rules/` when working on this project
- Apply project-specific patterns automatically

---

## 📖 Documentation

For complete SubAgent documentation, see:
- [SubAgent Integration Guide](../docs/subagent-integration-guide.md)
- [SubAgent Quick Reference](../docs/SUBAGENT_QUICKREF.md)
- [SubAgent Architecture](../docs/SUBAGENT_ARCHITECTURE.md)

---

**Remember:** `.cursor/` configures AI behavior for YOUR project. SubAgents are built-in Cursor features that READ this configuration.
