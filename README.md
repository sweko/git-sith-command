# git-sith

A developer-oriented Git power tool for common tasks. Because the dark side has better tools. ⚔️

## Features

### `git sith ignore`

Easily add `.gitignore` templates to your repository using GitHub's official template collection.

**Usage:**

```bash
# Add a single template
git sith ignore node

# Add multiple templates
git sith ignore node python visualstudio

# List all available templates
git sith ignore --list
```

## Installation

### Option 1: Install as a .NET Global Tool (Recommended)

```bash
# Build and install locally
dotnet pack
dotnet tool install --global --add-source ./nupkg GitSith

# Or install from NuGet (once published)
dotnet tool install --global GitSith
```

### Option 2: Build and Add to PATH

```bash
# Build the project
dotnet build -c Release

# Copy the executable to a directory in your PATH
# On Windows:
copy bin\Release\net8.0\git-sith.exe C:\YourPathDirectory\

# On Linux/macOS:
cp bin/Release/net8.0/git-sith /usr/local/bin/
chmod +x /usr/local/bin/git-sith
```

## Requirements

- .NET 8.0 SDK or later
- Git

## How It Works

Git automatically recognizes executables named `git-<subcommand>` in your PATH. When you run `git sith`, Git looks for an executable called `git-sith` and executes it with any additional arguments.

## Development

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run locally
dotnet run -- ignore node

# Run tests (when added)
dotnet test
```

## Roadmap

Future commands being considered:

- `git sith wip` - Quick WIP commits
- `git sith undo` - Safe undo operations
- `git sith clean` - Interactive cleanup
- `git sith prune` - Clean up branches
- `git sith stats` - Repository statistics

## Contributing

Ideas and contributions welcome!

## License

MIT
