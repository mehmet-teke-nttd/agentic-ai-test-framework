---
name: git-checkpoint
description: >-
  Quick save workflow for work-in-progress code. Automatically stages all changes,
  creates a timestamped WIP commit, and optionally pushes to remote backup branch.
  Use when the user wants to quickly save work, create a backup, or needs to switch
  branches without losing progress. Use when user says "checkpoint", "quick save",
  "backup my work", or "save WIP".
disable-model-invocation: true
---

# Git Checkpoint (Quick Save)

This skill provides a fast, automated workflow for saving work-in-progress without worrying about commit message quality or staging.

## Quick Start

When the user wants to quickly save work:

1. **Stage all changes** — `git add -A`
2. **Create WIP commit** — Timestamped commit message
3. **Optional: Push to backup branch** — For safety
4. **Provide cleanup guidance** — How to clean up later

---

## Basic Checkpoint Workflow

### Simple Checkpoint (Local Only)

```bash
# Stage everything
git add -A

# Create timestamped WIP commit
git commit -m "wip: checkpoint $(date +%Y-%m-%d_%H-%M-%S)"
```

**Resulting commit message:**
```
wip: checkpoint 2026-09-09_14-30-45
```

### Checkpoint with Remote Backup

```bash
# Stage and commit
git add -A
git commit -m "wip: checkpoint $(date +%Y-%m-%d_%H-%M-%S)"

# Push to remote (creates backup)
git push
```

### Checkpoint with Description

```bash
# Add context for yourself
git add -A
git commit -m "wip: dropdown tests in progress - selectors working, assertions pending"
git push
```

---

## When to Use Checkpoints

✅ **End of work session** — Save progress before closing laptop  
✅ **Before switching branches** — Don't lose uncommitted work  
✅ **Before risky refactoring** — Easy rollback point  
✅ **Before pulling updates** — Stash alternative  
✅ **Before experimenting** — Safe to try and revert  
✅ **Need to switch context urgently** — Quick save and switch  
✅ **As backup before rebasing** — Safety net  

❌ **DON'T use for:**
- Final commits to main/develop (use `git-commit` skill for proper messages)
- Pull request commits (clean up WIP commits first)
- Shared branches in active use by team

---

## Checkpoint Strategies

### Strategy 1: Frequent Local Checkpoints

**For:** Active development, experimentation  
**Pattern:** Commit locally every 15-30 minutes

```bash
# Every 15-30 minutes while coding
git add -A
git commit -m "wip: checkpoint $(date +%H-%M)"
```

**Cleanup later:**
```bash
# Squash all WIP commits into one proper commit
git rebase -i HEAD~5
# Mark all WIP commits as "squash" or "fixup"
# Write proper commit message
```

### Strategy 2: Checkpoint with Backup Branch

**For:** Long-running work, experimental features  
**Pattern:** Use dedicated backup branch

```bash
# Create backup branch
git checkout -b feature/dropdown-tests-backup
git checkout -b feature/dropdown-tests

# Work on feature branch
# ... make changes ...

# Checkpoint to backup
git add -A
git commit -m "wip: checkpoint $(date +%Y-%m-%d_%H-%M-%S)"
git push -u origin feature/dropdown-tests-backup
```

**Benefits:**
- Main feature branch stays clean
- Backup exists remotely
- Easy to recover from mistakes

### Strategy 3: Descriptive Checkpoints

**For:** Long work sessions, complex changes  
**Pattern:** Add brief context to WIP commits

```bash
git add -A
git commit -m "wip: extracted page object methods, need to update steps"
git commit -m "wip: steps updated, tests failing on selector"
git commit -m "wip: selector fixed, adding assertions"
```

**Cleanup:**
```bash
git rebase -i HEAD~3
# Squash all three into:
# feat(ui): extract dropdown page object and implement tests
```

---

## Checkpoint Before Risky Operations

### Before Rebase

```bash
# Checkpoint current state
git add -A
git commit -m "wip: checkpoint before rebase"

# Create backup branch
git branch backup-before-rebase

# Now safe to rebase
git rebase -i HEAD~10

# If rebase goes wrong
git rebase --abort
git reset --hard backup-before-rebase
```

### Before Merge

```bash
# Checkpoint current work
git add -A
git commit -m "wip: checkpoint before merge"

# Merge
git merge feature/other-branch

# If merge creates issues
git merge --abort
git reset --hard HEAD^
```

### Before Pull

```bash
# Checkpoint uncommitted work
git add -A
git commit -m "wip: checkpoint before pull"

# Pull changes
git pull --rebase origin main

# If conflicts are too complex
git rebase --abort
git reset --hard HEAD^
```

---

## Cleaning Up Checkpoints

### Squash WIP Commits

**Interactive rebase:**
```bash
# View last 5 commits
git log --oneline -5

# Output:
# abc123 wip: checkpoint 14-45
# def456 wip: checkpoint 14-30
# ghi789 wip: checkpoint 14-15
# jkl012 feat(ui): add dropdown page object
# mno345 Previous commit

# Squash the 3 WIP commits
git rebase -i HEAD~3

# In editor, change:
pick abc123 wip: checkpoint 14-45
pick def456 wip: checkpoint 14-30
pick ghi789 wip: checkpoint 14-15

# To:
pick abc123 wip: checkpoint 14-45
squash def456 wip: checkpoint 14-30
squash ghi789 wip: checkpoint 14-15

# Write proper commit message:
feat(ui): implement dropdown selection tests with page object

- Create DropdownPage with selection methods
- Implement step definitions with browser session
- Add comprehensive test scenarios
```

### Amend Into Previous Commit

```bash
# If checkpoint is just continuing previous work
git reset --soft HEAD^   # Undo checkpoint commit
git commit --amend --no-edit  # Add changes to previous commit
```

### Reset to Before Checkpoints

```bash
# Undo last 3 commits but keep changes
git reset --soft HEAD~3

# Now create one proper commit
git commit -m "feat(ui): implement dropdown tests"
```

---

## PowerShell Checkpoint Alias (Windows)

Add to PowerShell profile for quick checkpoint command:

```powershell
# Open profile
notepad $PROFILE

# Add function
function Git-Checkpoint {
    param([string]$message = "checkpoint")
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    git add -A
    git commit -m "wip: $message $timestamp"
    Write-Host "Checkpoint created: wip: $message $timestamp" -ForegroundColor Green
}

# Usage: Git-Checkpoint
# Usage: Git-Checkpoint "dropdown tests in progress"
```

**Use it:**
```powershell
Git-Checkpoint
Git-Checkpoint "before refactoring"
```

---

## Framework-Specific Checkpoint Patterns

### Checkpoint During Test Development

```bash
# After creating feature file
git add tests/UI.Tests/Features/Demo/DemoDropdowns.feature
git commit -m "wip: feature file for dropdown tests"

# After creating page object
git add tests/UI.Tests/PageObjects/DemoDropdownsPage.cs
git commit -m "wip: dropdown page object with selectors"

# After creating steps
git add tests/UI.Tests/Steps/Demo/DemoDropdownsSteps.cs
git commit -m "wip: dropdown step definitions, need assertions"

# Before final commit, squash all WIP commits
git rebase -i HEAD~3
# Result:
feat(ui): add dropdown selection test suite

Complete test implementation following POM pattern:
- Feature file with smoke and regression scenarios
- Page object with dropdown interaction methods
- Step definitions with browser session handling
```

### Checkpoint During Refactoring

```bash
# Checkpoint before starting
git add -A
git commit -m "wip: before POM refactor of registration tests"

# After extracting page object
git add tests/UI.Tests/PageObjects/RegistrationPage.cs
git commit -m "wip: extracted registration page object"

# After updating steps
git add tests/UI.Tests/Steps/Checkout/RegistrationSteps.cs
git commit -m "wip: updated steps to use page object"

# After testing
git add -A
git commit -m "wip: tests passing"

# Clean up
git rebase -i HEAD~4
# Result:
refactor(ui): migrate registration tests to POM pattern

- Extract page logic to RegistrationPage
- Remove direct Playwright calls from steps
- Add reusable methods for form interactions

No behavior changes; improves maintainability
```

---

## Emergency Recovery Scenarios

### Accidentally Deleted Code

```bash
# If you committed checkpoint before deleting
git log --oneline
# Find checkpoint commit

# Restore file from checkpoint
git checkout abc123 -- path/to/file.cs
```

### Experiment Went Wrong

```bash
# Return to last checkpoint
git log --oneline
git reset --hard abc123  # Reset to checkpoint commit
```

### Need to Switch Branches Urgently

```bash
# Quick checkpoint
git add -A
git commit -m "wip: urgent checkpoint before branch switch"

# Switch branches
git checkout other-branch

# Later, return and continue
git checkout original-branch
```

---

## Best Practices

### ✅ DO

- Use checkpoints liberally during active development
- Add brief descriptions to checkpoints for context
- Clean up checkpoints before pushing to shared branches
- Push checkpoints to backup branches if needed
- Use checkpoints before risky operations

### ❌ DON'T

- Push WIP commits to main or develop branches
- Leave WIP commits in pull requests
- Use checkpoints as final commit messages
- Skip cleanup before code review

---

## Quick Reference

### Basic Commands
```bash
# Simple checkpoint
git add -A && git commit -m "wip: checkpoint $(date +%H-%M)"

# Checkpoint with description
git add -A && git commit -m "wip: dropdown tests - selectors working"

# Checkpoint and push
git add -A && git commit -m "wip: checkpoint" && git push

# View checkpoints
git log --oneline --grep="wip:"

# Clean up last 3 checkpoints
git rebase -i HEAD~3
```

### Recovery
```bash
# Undo last checkpoint (keep changes)
git reset --soft HEAD^

# Undo last checkpoint (discard changes)
git reset --hard HEAD^

# Return to specific checkpoint
git reset --hard abc123
```

---

## Integration with Other Skills

| Skill | Relationship |
|-------|-------------|
| `git-commit` | Use for final, proper commits |
| `git-push` | Use after cleaning up checkpoints |

**Workflow:**
```
Development → git-checkpoint (frequent) → Cleanup → git-commit (proper) → git-push
```

---

## Checklist

Before using checkpoint:

- [ ] Know this is temporary (will clean up later)
- [ ] Not pushing to shared branch without cleanup
- [ ] Understand how to squash/clean later

Before cleaning up checkpoints:

- [ ] All work is complete and tested
- [ ] Ready to write proper commit message
- [ ] Know which commits to squash
- [ ] Have proper commit message ready

---

## When to Use This Skill

✅ Need to save work quickly  
✅ End of work day/session  
✅ Before switching branches  
✅ Before risky operations (rebase, merge)  
✅ During experimental development  
✅ User says "checkpoint", "quick save", "backup"  

## When NOT to Use

❌ Creating final commits for PR  
❌ Committing to main/develop  
❌ Code is ready for review  
❌ Want proper commit message (use `git-commit` instead)  
