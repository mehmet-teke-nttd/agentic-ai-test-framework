# .cursor Folder Explained - Visual Guide

**The #1 Question:** Do I need to define SubAgents in `.cursor/`?  
**The Answer:** ❌ **NO!** SubAgents are built into Cursor IDE.

---

## 🎯 The Confusion

Many developers think:
```
❌ WRONG: "I need to create SubAgent files in .cursor/"
❌ WRONG: "SubAgents are defined in my project"
❌ WRONG: "I need to configure SubAgents to use them"
```

**The Truth:**
```
✅ CORRECT: SubAgents are built into Cursor IDE
✅ CORRECT: .cursor/ contains PROJECT rules, not SubAgent definitions
✅ CORRECT: SubAgents READ your .cursor/ rules to understand YOUR project
```

---

## 🏗️ Visual Architecture

```
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃            CURSOR IDE (The Application)                  ┃
┃                                                           ┃
┃  ╔══════════════════════════════════════════════════╗   ┃
┃  ║  BUILT-IN SUBAGENTS (Come with Cursor)          ║   ┃
┃  ║                                                  ║   ┃
┃  ║  📦 explore        - Code discovery             ║   ┃
┃  ║  📦 bugbot         - Quality review             ║   ┃
┃  ║  📦 security-review - Security audit            ║   ┃
┃  ║  📦 ci-investigator - CI debugging              ║   ┃
┃  ║  📦 generalPurpose - Complex tasks              ║   ┃
┃  ║                                                  ║   ┃
┃  ║  ❌ NOT defined by you                          ║   ┃
┃  ║  ✅ Available to ALL Cursor projects            ║   ┃
┃  ╚══════════════════════════════════════════════════╝   ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━┳━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                             │
                             │ Reads configuration from
                             │
                             ▼
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃        YOUR PROJECT (Your Codebase)                      ┃
┃                                                           ┃
┃  ╔══════════════════════════════════════════════════╗   ┃
┃  ║  .cursor/ FOLDER (Your Project Rules)           ║   ┃
┃  ║                                                  ║   ┃
┃  ║  📁 rules/                                       ║   ┃
┃  ║     └── AGENTS.md    ← "POM pattern required"   ║   ┃
┃  ║     └── RULE.md      ← "Naming: {Feature}*"     ║   ┃
┃  ║                                                  ║   ┃
┃  ║  📁 skills/                                      ║   ┃
┃  ║     └── tests-from-user-story/                  ║   ┃
┃  ║                                                  ║   ┃
┃  ║  📁 hooks/                                       ║   ┃
┃  ║     └── hooks.json   ← "Run bugbot pre-commit"  ║   ┃
┃  ║                                                  ║   ┃
┃  ║  ✅ Defined by you                               ║   ┃
┃  ║  ✅ Specific to THIS project                     ║   ┃
┃  ╚══════════════════════════════════════════════════╝   ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
```

---

## 🔄 How It Actually Works

### Step 1: You Ask for Help
```
You: "Review my UI test with Bugbot"
```

### Step 2: I Launch the Built-in SubAgent
```
Me (AI Assistant): *Launches bugbot SubAgent*
                    ↓
        (bugbot is built into Cursor IDE)
```

### Step 3: SubAgent Reads Your Project Rules
```
Bugbot: *Reads .cursor/rules/AGENTS.md*
        "Oh, this project requires:
         - POM pattern
         - Naming: {Feature}.feature → {Feature}Steps.cs
         - No direct Playwright calls in Steps"
```

### Step 4: SubAgent Reviews Using YOUR Rules
```
Bugbot: *Analyzes your code*
        "Found 2 direct Playwright calls in Steps
         This violates .cursor/rules/AGENTS.md
         → VIOLATION"
```

---

## 📊 Side-by-Side Comparison

| Aspect | SubAgents | .cursor/ Folder |
|--------|-----------|-----------------|
| **What is it?** | Tools built into Cursor IDE | Configuration files in your project |
| **Where is it?** | Inside Cursor application | In your codebase |
| **Who creates it?** | Cursor developers | You (the project team) |
| **Can you modify it?** | No - it's part of Cursor | Yes - it's your configuration |
| **Scope** | Available to ALL Cursor projects | Only affects THIS project |
| **Purpose** | Provide AI capabilities | Tell AI about YOUR project rules |
| **Examples** | explore, bugbot, ci-investigator | AGENTS.md, hooks.json, SKILL.md |

---

## 🎯 What You Actually Define

### ✅ You Define (in .cursor/)

#### 1. **Project Rules** (`.cursor/rules/`)
```markdown
# AGENTS.md
All UI tests MUST follow Page Object Model pattern.
Naming: {Feature}.feature → {Feature}Steps.cs → {Feature}Page.cs
```

**Purpose:** Tell SubAgents how YOUR project works

#### 2. **Code Generation Templates** (`.cursor/skills/`)
```markdown
# SKILL.md
When generating UI tests:
1. Create Feature file
2. Create Steps file
3. Create Page Object
All following POM pattern
```

**Purpose:** Templates for generating code YOUR way

#### 3. **Automation Triggers** (`.cursor/hooks/`)
```json
{
  "hooks": [
    {
      "id": "pre-commit-review",
      "action": {
        "type": "agent",
        "subagentType": "bugbot"
      }
    }
  ]
}
```

**Purpose:** When to automatically trigger SubAgents

---

### ❌ You DON'T Define

#### SubAgent Capabilities
```
❌ You can't create new SubAgent types
❌ You can't modify what SubAgents do
❌ You can't change how SubAgents work
```

**These are built into Cursor IDE**

---

## 🧪 Real Example

### What You Might Think:
```
"I need to create a file: .cursor/subagents/bugbot.json
to define the bugbot SubAgent"
```

### What Actually Happens:
```
1. Bugbot already exists in Cursor IDE
2. You just need .cursor/rules/AGENTS.md
3. Bugbot reads AGENTS.md to understand YOUR rules
4. That's it! No SubAgent definition needed.
```

---

## 📁 Actual .cursor/ Contents

Here's what's REALLY in your `.cursor/` folder:

```
.cursor/
│
├── rules/                    ← AI GUIDELINES
│   ├── AGENTS.md             ← "How to code in this project"
│   └── RULE.md               ← "POM pattern details"
│
├── skills/                   ← CODE GENERATION TEMPLATES
│   ├── expand-test-idea/
│   │   └── SKILL.md          ← "How to expand test ideas"
│   ├── tests-from-user-story/
│   │   └── SKILL.md          ← "How to convert user stories"
│   └── ui-tests-from-screenshot/
│       └── SKILL.md          ← "How to generate from screenshots"
│
├── hooks/                    ← AUTOMATION TRIGGERS
│   └── hooks.json            ← "When to trigger SubAgents"
│
└── README.md                 ← EXPLANATION (just created!)
```

**Notice:** No "subagents/" folder. No SubAgent definitions. Just project configuration!

---

## 🎓 Analogy to Understand It

Think of it like a restaurant:

### SubAgents = Kitchen Equipment (Built-in)
```
🔪 Knife (explore SubAgent)
🍳 Pan (bugbot SubAgent)
🥄 Spoon (ci-investigator SubAgent)

These come WITH the kitchen (Cursor IDE).
You don't build them yourself.
```

### .cursor/ = Recipe Book (Your Configuration)
```
📖 Recipe: "Italian Pasta"
   - Use knife (explore) to check ingredients
   - Use pan (bugbot) to cook
   - Use YOUR specific sauce recipe

The recipe book tells the chef HOW to use
the equipment for YOUR specific dishes.
```

**SubAgents are the tools. `.cursor/` is the instruction manual for YOUR project.**

---

## ✅ Summary Checklist

Understanding Check:

- [ ] ✅ SubAgents are built into Cursor IDE
- [ ] ✅ I don't need to create SubAgent files
- [ ] ✅ `.cursor/` contains MY project rules
- [ ] ✅ SubAgents READ my `.cursor/` rules
- [ ] ✅ I configure `.cursor/`, not SubAgents themselves

---

## 🚀 What You Should Do

### ✅ DO:
1. **Create/update `.cursor/rules/`** with YOUR project rules
2. **Create `.cursor/skills/`** with YOUR code templates
3. **Configure `.cursor/hooks/`** for automation
4. **Use SubAgents** by asking for them in chat

### ❌ DON'T:
1. Try to create SubAgent definition files
2. Try to modify SubAgent behavior
3. Look for SubAgent configuration in Cursor
4. Think you need to "install" SubAgents

---

## 💡 Key Takeaway

```
┌──────────────────────────────────────────┐
│  SubAgents = Built into Cursor          │ ← You just use them
├──────────────────────────────────────────┤
│  .cursor/ = Your project configuration   │ ← You define this
└──────────────────────────────────────────┘
```

**SubAgents are like built-in calculators on your phone.**  
**`.cursor/` is like your personal preferences for how to use them.**

---

## 🎯 Your Next Steps

1. **Stop worrying about "defining SubAgents"** - they already exist!

2. **Focus on `.cursor/rules/`** - Make sure AGENTS.md describes YOUR project

3. **Try using a SubAgent:**
   ```
   "Explore my test framework structure"
   ```

4. **Watch how it reads YOUR `.cursor/rules/`** to understand YOUR project

---

## 📞 Still Confused?

Ask yourself:
- "Do I need to define Microsoft Word before using it?" → No!
- "Do I need to define SubAgents before using them?" → No!

SubAgents are like Word - they're already there. You just use them!

---

**Bottom Line:** `.cursor/` is for YOUR rules, not SubAgent definitions. SubAgents are already built into Cursor! 🚀
