# Quick Start Guide

## Getting Started with git-sith

### 1. Build the Project

```bash
cd GitSith
dotnet restore
dotnet build
```

### 2. Test It Out

```bash
# Run directly with dotnet
dotnet run -- ignore --list

# This will show all available .gitignore templates
```

### 3. Install as Global Tool

```bash
# Pack the project
dotnet pack -c Release

# Install globally
dotnet tool install --global --add-source ./bin/Release GitSith

# Now you can use it anywhere!
git sith ignore --list
```

### 4. Try It in a Git Repository

```bash
# Navigate to any git repository
cd /path/to/your/repo

# Add some templates
git sith ignore node
git sith ignore python visualstudio

# Check your .gitignore file
cat .gitignore
```

## Uninstall

```bash
dotnet tool uninstall --global GitSith
```

## Troubleshooting

### "git sith" command not found

Make sure the .NET tools directory is in your PATH:

**Windows:**
```
%USERPROFILE%\.dotnet\tools
```

**Linux/macOS:**
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
# Add this to your ~/.bashrc or ~/.zshrc
```

### Templates not fetching

The tool fetches templates from GitHub's API at `https://api.github.com/gitignore/templates`.
Make sure you have internet connectivity and GitHub is accessible.

## Current Features

- ✅ Fetch .gitignore templates from GitHub's official collection
- ✅ Add single or multiple templates at once
- ✅ List all available templates
- ✅ Automatic headers for each template section

## Future Ideas

Add your own ideas! Some possibilities:

- Cache templates locally for offline use
- Custom template management
- Interactive template selection
- Merge/deduplicate existing .gitignore entries
- Template search/filter functionality
