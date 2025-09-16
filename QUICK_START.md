# 🚀 InfoPoint Quick Start for Your Friend

## Repository is now PUBLIC! ✅
**URL**: https://github.com/OHBDC/Infopoint

## Your Assignment: Learning Walks 📋
- **Branch**: `feature/learning-walks`
- **Work Area**: `Areas/LearningWalks/`
- **Focus**: Classroom observation features

## Quick Setup (5 minutes):

### 1. Clone Repository
```bash
git clone https://github.com/OHBDC/Infopoint.git
cd Infopoint
```

### 2. Setup Project
```bash
dotnet restore
dotnet ef database update
```

### 3. Switch to Your Branch
```bash
git fetch --all
git checkout feature/learning-walks
```

### 4. Test It Works
```bash
dotnet run --urls="http://localhost:5050;https://localhost:5051"
```

## Your Daily Workflow:
```bash
# Start of day
git checkout feature/learning-walks
git pull origin dev

# Work in Areas/LearningWalks/ folder
# Build learning walk features

# End of day
git add .
git commit -m "Describe what you built"
git push origin feature/learning-walks
```

## 📚 Full Documentation:
- **Complete Setup**: Read `DEVELOPMENT_SETUP.md`
- **Git Commands**: Check `GIT_WORKFLOW.md`

## Need Help?
- Check the documentation files
- Ask your development partner (PDRs developer)
- Look at existing code patterns

**Happy coding!** 🎉