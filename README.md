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

# Use aliases (csharp → Dotnet, js → Node, etc.)
git sith ignore csharp js

# List all available templates
git sith ignore --list

# Show available aliases
git sith ignore --aliases

# Embrace the dark side (ignore everything)
git sith ignore
```

### `git sith force-push` / `git sith push`

Stage all changes, commit with a message, and push to remote in one command. Perfect for quick iterations.

**Usage:**

```bash
# Commit with a custom message and push
git sith push "Fixed the bug"

# Let the dark side choose your commit message
git sith push
# Uses random Sith quotes like "Peace is a lie, there is only passion."
```

**Aliases:** `force`, `force-push`, `push`

### `git sith purge` / `git sith order-66`

Remove a file from git history completely - the nuclear option. Rewrites history to eliminate all traces of a file.

**Usage:**

```bash
# Purge a file from all history
git sith purge secrets.txt

# Use the Order 66 alias
git sith order-66 passwords.env

# With confirmation prompt (for the weak-willed)
git sith purge secrets.txt --confirm
```

**Aliases:** `purge`, `obliterate`, `destroy`, `66`, `order-66`, `damnatio-memoriae`, `memory-hole`

⚠️ **Warning:** This rewrites git history. After purging, you'll need to force push and collaborators will need to re-clone.

### `git sith help`

Show help for git-sith commands. Works around the issue where `git sith --help` is intercepted by Git itself.

**Usage:**

```bash
# Show all commands
git sith help

# Show help for a specific command
git sith help ignore
git sith help purge
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
- `git sith stats` - Repository statistics
- `git sith alias` - Manage git aliases with Sith flair

## Contributing

Ideas and contributions welcome!

## License

MIT
