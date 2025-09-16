# InfoPoint Development Setup Guide

This guide will help you and your development partner set up the InfoPoint project for collaborative development.

## Branch Structure

We've set up the following branch structure for organized development:

```
main                    # Production-ready code (protected)
├── dev                 # Integration branch for testing
├── feature/learning-walks   # Developer 1: Learning Walks features
└── feature/pdrs            # Developer 2: PDRs features
```

## Initial Setup for Your Development Partner

### Step 1: Clone the Repository

```bash
git clone https://github.com/OHBDC/Infopoint.git
cd Infopoint
```

### Step 2: Install Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

2. **Install Visual Studio Code**
   - Download from [code.visualstudio.com](https://code.visualstudio.com/)
   - Install C# extension (ms-dotnettools.csharp)

3. **Verify Installation**
   ```bash
   dotnet --version  # Should show 8.0.x
   ```

### Step 3: Set Up the Project

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Create the database:**
   ```bash
   dotnet ef database update
   ```

3. **Test the application:**
   ```bash
   dotnet run --urls="http://localhost:5050;https://localhost:5051"
   ```

### Step 4: Configure Git

```bash
# Set up your Git identity
git config --global user.name "Your Name"
git config --global user.email "your.email@g.bdc.ac.uk"

# Fetch all branches
git fetch --all

# See all available branches
git branch -a
```

## Developer Assignments

### Developer 1 (You) - Learning Walks
- **Branch**: `feature/learning-walks`
- **Area**: `Areas/LearningWalks/`
- **Responsibilities**:
  - Learning walk scheduling
  - Observation forms
  - Reporting features
  - Learning walks dashboard

### Developer 2 - PDRs
- **Branch**: `feature/pdrs`
- **Area**: `Areas/PDRs/`
- **Responsibilities**:
  - PDR scheduling
  - Objective setting
  - Performance tracking
  - PDR reports

## Daily Development Workflow

### For Developer 1 (Learning Walks):

```bash
# 1. Switch to your feature branch
git checkout feature/learning-walks

# 2. Pull latest changes from dev branch
git pull origin dev

# 3. Work on your features in Areas/LearningWalks/

# 4. Commit your changes
git add .
git commit -m "Add learning walk scheduling feature"

# 5. Push to your feature branch
git push origin feature/learning-walks
```

### For Developer 2 (PDRs):

```bash
# 1. Switch to your feature branch
git checkout feature/pdrs

# 2. Pull latest changes from dev branch
git pull origin dev

# 3. Work on your features in Areas/PDRs/

# 4. Commit your changes
git add .
git commit -m "Add PDR objective setting feature"

# 5. Push to your feature branch
git push origin feature/pdrs
```

## VS Code Git Integration

### Setting Up GitHub in VS Code

1. **Install GitHub Extension**: Search for "GitHub Pull Requests and Issues" in Extensions
2. **Sign In**: Command Palette (Ctrl+Shift+P) → "GitHub: Sign in"
3. **Clone Repository**: Command Palette → "Git: Clone" → Enter repository URL

### Working with Branches in VS Code

1. **Switch Branches**: Click branch name in status bar (bottom left)
2. **Create New Branch**: Command Palette → "Git: Create Branch"
3. **Push Changes**: Source Control panel → Click "..." → Push
4. **Pull Changes**: Source Control panel → Click "..." → Pull

### Useful VS Code Git Features

- **Source Control Panel**: Ctrl+Shift+G
- **View Changes**: Click on modified files in Source Control
- **Stage Changes**: Click "+" next to files
- **Commit**: Type message and click "✓"

## Pull Request Workflow

### When Feature is Ready

1. **Push your feature branch:**
   ```bash
   git push origin feature/your-branch
   ```

2. **Create Pull Request**:
   - Go to GitHub → Your repository
   - Click "Compare & pull request"
   - Set base branch to `dev` (NOT main)
   - Add description of changes
   - Request review from other developer

3. **Code Review Process**:
   - Other developer reviews code
   - Address feedback if needed
   - Once approved, merge to `dev`

### Merging to Main

- Only merge to `main` after both features are tested together in `dev`
- Create PR from `dev` → `main`
- Both developers review before merging

## Conflict Resolution

### If you get merge conflicts:

```bash
# 1. Pull latest changes from dev
git checkout dev
git pull origin dev

# 2. Switch to your feature branch
git checkout feature/your-branch

# 3. Merge dev into your branch
git merge dev

# 4. Resolve conflicts in VS Code
# VS Code will highlight conflicts - choose which changes to keep

# 5. Commit the merge
git commit -m "Resolve merge conflicts with dev"

# 6. Push resolved branch
git push origin feature/your-branch
```

## Database Changes

### If you add new models or change existing ones:

```bash
# Create migration
dotnet ef migrations add YourMigrationName

# Update database
dotnet ef database update

# Commit migration files
git add Migrations/
git commit -m "Add migration for new feature"
```

### Sharing Database Changes

- **Always commit migration files**
- **Other developer runs**: `dotnet ef database update`
- **Never delete other developer's migrations**

## Testing Your Changes

### Before Committing:

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run the application:**
   ```bash
   dotnet run --urls="http://localhost:5050;https://localhost:5051"
   ```

3. **Test your area:**
   - Navigate to your specific area
   - Test all functionality
   - Ensure no errors in browser console

## Best Practices

### Commit Messages
```bash
# Good commit messages
git commit -m "Add learning walk creation form"
git commit -m "Fix PDR date validation bug"
git commit -m "Update learning walks navigation"

# Bad commit messages
git commit -m "stuff"
git commit -m "fixes"
git commit -m "wip"
```

### File Organization
- **Only modify files in your assigned area**
- **Shared files**: Discuss changes with other developer first
- **Layout changes**: Coordinate to avoid conflicts

### Communication
- **Daily sync**: Discuss what you're working on
- **Before major changes**: Check with other developer
- **Pull requests**: Review each other's code
- **Issues**: Use GitHub Issues to track bugs/features

## Troubleshooting

### Common Issues

1. **"Branch not found"**: Run `git fetch --all`
2. **"Authentication failed"**: Set up GitHub token in VS Code
3. **"Database error"**: Run `dotnet ef database update`
4. **"Build failed"**: Check for missing packages with `dotnet restore`

### Getting Help

- **Check this guide first**
- **Look at existing code examples**
- **Ask your development partner**
- **Check GitHub Issues**

## Quick Reference Commands

```bash
# Daily workflow
git checkout feature/your-branch
git pull origin dev
# Work on code
git add .
git commit -m "Description of changes"
git push origin feature/your-branch

# Branch management
git branch -a                    # List all branches
git checkout branch-name         # Switch branch
git pull origin dev              # Get latest from dev

# Emergency fixes
git stash                        # Save work temporarily
git checkout dev                 # Switch to dev
git pull origin dev              # Get latest
git checkout feature/your-branch # Switch back
git stash pop                    # Restore your work
```

---

**Remember**: Communication is key! Always coordinate with your development partner when making changes that might affect shared components.