# Git Workflow Quick Reference

## 🏗️ Branch Structure

```
main (🔒 protected)
├── dev (integration)
├── feature/learning-walks (Developer 1)
└── feature/pdrs (Developer 2)
```

## 🚀 Daily Workflow

### Developer 1 (PDRs - You)
```bash
git checkout feature/pdrs  
git pull origin dev
# Work in Areas/PDRs/
git add .
git commit -m "Your changes"
git push origin feature/pdrs
```

### Developer 2 (Learning Walks - Your Friend)
```bash
git checkout feature/learning-walks
git pull origin dev
# Work in Areas/LearningWalks/
git add .
git commit -m "Your changes"
git push origin feature/learning-walks
```

## 📋 VS Code Quick Actions

| Action | VS Code Method |
|--------|---------------|
| Switch Branch | Click branch name in status bar |
| Stage Changes | Source Control → Click "+" |
| Commit | Type message → Click "✓" |
| Push | Source Control → "..." → Push |
| Pull | Source Control → "..." → Pull |
| Create Branch | Ctrl+Shift+P → "Git: Create Branch" |

## 🔄 Pull Request Process

1. **Push feature branch**
2. **GitHub → Compare & pull request**
3. **Base: dev ← Compare: feature/your-branch**
4. **Request review from partner**
5. **Merge after approval**

## 🛠️ Essential Commands

```bash
# Setup (one-time)
git clone https://github.com/OHBDC/Infopoint.git
git fetch --all
dotnet restore
dotnet ef database update

# Daily start
git checkout feature/your-branch
git pull origin dev

# Daily end
git add .
git commit -m "Descriptive message"
git push origin feature/your-branch

# Database changes
dotnet ef migrations add MigrationName
dotnet ef database update
git add Migrations/
git commit -m "Add migration: description"
```

## ⚠️ Conflict Resolution

```bash
git checkout dev
git pull origin dev
git checkout feature/your-branch
git merge dev
# Fix conflicts in VS Code
git commit -m "Resolve conflicts"
git push origin feature/your-branch
```

## 🆘 Emergency Commands

```bash
# Save work temporarily
git stash
git checkout other-branch
git stash pop  # to restore

# Undo last commit (keep changes)
git reset --soft HEAD~1

# Discard all local changes
git checkout -- .
```

## 📝 Good Commit Messages

✅ **Good:**
- `Add learning walk creation form`
- `Fix PDR validation bug`  
- `Update navigation for mobile`

❌ **Bad:**
- `stuff`
- `fixes`
- `wip`

## 🎯 Developer Areas

| Developer | Branch | Focus Area | Files |
|-----------|--------|------------|-------|
| Developer 1 (You) | `feature/pdrs` | PDRs | `Areas/PDRs/` |
| Developer 2 (Friend) | `feature/learning-walks` | Learning Walks | `Areas/LearningWalks/` |

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| Authentication failed | Set up GitHub token in VS Code |
| Branch not found | `git fetch --all` |
| Build errors | `dotnet restore` |
| Database errors | `dotnet ef database update` |

---
**💡 Pro Tip**: Always pull from `dev` before starting work to avoid conflicts!