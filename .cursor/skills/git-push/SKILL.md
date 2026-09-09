---
name: git-push
description: >-
  Push code to remote repository with pre-push validations including build checks,
  test execution, and lint verification. Ensures code quality before pushing to shared
  branches. Use when pushing code, uploading commits, syncing with remote, or when the
  user asks to "push" or "push to origin".
disable-model-invocation: true
---

# Git Push with Validations

This skill helps safely push code to remote repositories with optional pre-push quality checks.

## Quick Start

When the user wants to push code:

1. **Check status** — Verify there are commits to push
2. **Run validations** (optional but recommended) — Build, test, lint
3. **Push to remote** — Execute push command
4. **Verify success** — Confirm remote is updated

---

## Basic Push Workflow

### 1. Check Local Status

```bash
git status
```

Verify:
- Working directory is clean
- Local branch is ahead of remote
- No uncommitted changes (or commit them first)

### 2. Check Remote Status

```bash
git fetch origin
git status
```

Check if:
- Local is ahead: Safe to push
- Local is behind: Need to pull first
- Branches have diverged: Need to merge or rebase

### 3. Push to Remote

**Push current branch:**
```bash
git push
```

**Push and set upstream (first push of new branch):**
```bash
git push -u origin branch-name
```

**Push specific branch:**
```bash
git push origin feature/my-feature
```

**Force push (use with extreme caution):**
```bash
git push --force-with-lease origin branch-name
```

---

## Pre-Push Validations

### Validation Levels

Choose validation level based on branch and risk:

| Branch Type | Validation Level | Checks |
|-------------|------------------|--------|
| `main`, `develop` | **Strict** | Build + All tests + Lint |
| `feature/*` | **Standard** | Build + Smoke tests |
| `bugfix/*` | **Standard** | Build + Affected tests |
| `hotfix/*` | **Strict** | Build + All tests + Lint |
| Personal WIP | **Minimal** | Build only |

### Build Validation

```bash
dotnet build
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

If build fails:
- ❌ **DO NOT PUSH**
- Fix build errors
- Run `dotnet build` again
- Only push after successful build

### Test Validation

**Run smoke tests (fast):**
```bash
dotnet test --filter "Category=Smoke"
```

**Run all tests (comprehensive):**
```bash
dotnet test
```

**Run specific test project:**
```bash
dotnet test tests/UI.Tests/UI.Tests.csproj
dotnet test tests/API.Tests/API.Tests.csproj
dotnet test tests/Integration.Tests/Integration.Tests.csproj
```

If tests fail:
- ❌ **DO NOT PUSH** (unless isolated test environment issue)
- Investigate failures
- Fix failing tests or broken code
- Re-run tests
- Push after all tests pass

### Lint Validation

```bash
dotnet format --verify-no-changes
```

If format issues found:
```bash
dotnet format
git add -A
git commit -m "style: apply code formatting"
```

---

## Push Strategies by Scenario

### Scenario 1: First Push of New Branch

```bash
# Create and switch to new branch
git checkout -b feature/add-dropdown-tests

# Make changes, commit
git add .
git commit -m "feat(ui): add dropdown selection tests"

# First push - set upstream
git push -u origin feature/add-dropdown-tests
```

### Scenario 2: Pushing to Existing Branch

```bash
# Make changes, commit
git commit -m "fix(ui): correct dropdown selector"

# Push (upstream already set)
git push
```

### Scenario 3: Local Behind Remote

```bash
git fetch origin
git status
# Output: Your branch is behind 'origin/main' by 3 commits

# Pull changes first
git pull --rebase origin main

# Resolve conflicts if any
# Then push
git push
```

### Scenario 4: Branches Diverged

```bash
git status
# Output: Your branch and 'origin/main' have diverged

# Option 1: Rebase (preferred for clean history)
git pull --rebase origin main
# Resolve conflicts
git push

# Option 2: Merge (preserves both histories)
git pull origin main
# Resolve conflicts
git push
```

### Scenario 5: Force Push (Dangerous)

**Only use when:**
- You've rebased local commits
- You're working alone on the branch
- You need to fix pushed commit history

```bash
# Safer force push (fails if remote has changes you don't have)
git push --force-with-lease origin feature-branch

# Nuclear force push (overwrites remote, dangerous)
git push --force origin feature-branch
```

⚠️ **Never force push to:**
- `main` or `master`
- `develop`
- Any shared branch others are working on

---

## Framework-Specific Workflows

### Pushing Test Changes

**Standard workflow:**
```bash
# 1. Verify build
dotnet build

# 2. Run affected tests
dotnet test --filter "Feature=DropdownSelection"

# 3. Run smoke tests
dotnet test --filter "Category=Smoke"

# 4. If all pass, push
git push
```

### Pushing Framework Core Changes

```bash
# 1. Build entire solution
dotnet build

# 2. Run all test layers
dotnet test tests/UI.Tests/
dotnet test tests/API.Tests/
dotnet test tests/Integration.Tests/

# 3. Run framework unit tests
dotnet test tests/TestFramework.UnitTests/

# 4. If all pass, push
git push
```

### Pushing Documentation Only

```bash
# Documentation changes don't require test runs
dotnet build  # Quick verification
git push
```

---

## Handling Push Failures

### Remote Rejected (Behind)

```bash
# Error: Updates were rejected because the remote contains work that you do not have locally

# Solution: Pull and merge
git pull --rebase origin main
git push
```

### Pre-Receive Hook Failure

```bash
# Error: pre-receive hook declined

# Common causes:
# - Build failures
# - Test failures
# - Lint issues
# - Protected branch rules

# Solution: Fix the issue locally
dotnet build
dotnet test
git push
```

### Authentication Failures

```bash
# Error: Authentication failed

# Solution 1: Update credentials
git config --global credential.helper manager

# Solution 2: Use SSH instead of HTTPS
git remote set-url origin git@github.com:user/repo.git

# Solution 3: Generate new token (GitHub/Azure DevOps)
# Follow platform-specific token generation process
```

---

## Best Practices

### Before Every Push

1. ✅ **Commit all changes** — No uncommitted files
2. ✅ **Build successfully** — `dotnet build` passes
3. ✅ **Tests pass** — At least smoke tests
4. ✅ **Pull latest** — No divergence with remote
5. ✅ **Review commits** — `git log origin/main..HEAD`

### Branch Protection

If pushing to protected branches:

```bash
# Check branch protection rules first
git remote show origin

# Follow PR workflow instead of direct push
git push -u origin feature/my-feature
# Then create PR via GitHub/Azure DevOps UI
```

### Push Frequency

**Recommended:**
- Push after each logical unit of work
- Push at end of work session
- Push before switching branches
- Push before rebasing or history changes

**Avoid:**
- Pushing untested code to shared branches
- Pushing broken builds
- Pushing secrets or sensitive data

---

## Quick Reference Commands

### Status and Info
```bash
git status                          # Check working directory
git log origin/main..HEAD           # Commits to be pushed
git diff origin/main...HEAD         # Changes to be pushed
git remote -v                       # Show remotes
```

### Common Push Commands
```bash
git push                            # Push current branch
git push -u origin branch-name      # Push and set upstream
git push origin --delete branch-name # Delete remote branch
git push --tags                     # Push tags
git push --force-with-lease         # Safe force push
```

### Pre-Push Checks
```bash
dotnet build                        # Build check
dotnet test --filter "Category=Smoke" # Smoke tests
dotnet format --verify-no-changes   # Lint check
git fetch origin && git status      # Check remote status
```

---

## Integration with Other Skills

| Skill | Use Together |
|-------|--------------|
| `git-commit` | Commit first, then push |
| `git-checkpoint` | Checkpoint includes auto-push |

---

## Common Mistakes to Avoid

❌ **Pushing without building**
```bash
git push  # Without running dotnet build first
```

❌ **Pushing failing tests**
```bash
# 3 tests failed
git push  # DON'T DO THIS
```

❌ **Force pushing to main**
```bash
git push --force origin main  # NEVER DO THIS
```

❌ **Pushing without pulling**
```bash
# Remote has changes you don't have
git push  # Will fail or cause divergence
```

✅ **Good workflow**
```bash
dotnet build
dotnet test --filter "Category=Smoke"
git pull --rebase origin main
git push
```

---

## Checklist

Before pushing, verify:

- [ ] All changes are committed (`git status` clean)
- [ ] Build succeeds (`dotnet build`)
- [ ] Tests pass (`dotnet test`)
- [ ] Local is up to date with remote (`git pull`)
- [ ] Commit messages follow conventions
- [ ] No sensitive data in commits
- [ ] Branch name follows conventions (feature/, bugfix/, etc.)

---

## When to Use This Skill

✅ Pushing committed changes to remote  
✅ User asks to "push code" or "push to origin"  
✅ Syncing local branch with remote  
✅ Need validation before pushing  
✅ Troubleshooting push failures  

## When NOT to Use

❌ No commits to push  
❌ Build or tests failing (fix first)  
❌ Working directory has uncommitted changes (commit first)  
❌ Creating pull requests (use platform UI after push)  
