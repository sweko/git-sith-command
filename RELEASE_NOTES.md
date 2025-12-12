# Release Notes

## Version 0.0.5 (Current)

### New Features

- **`git sith force-push`** - Stage all changes, commit, and push in one command
  - Random Sith quotes as default commit messages
  - Aliases: `force`, `force-push`, `push`

- **`git sith purge`** - Remove files from git history completely
  - Uses `git-filter-repo` (preferred) or `git filter-branch`
  - Multiple Sith-themed aliases: `order-66`, `damnatio-memoriae`, `memory-hole`, `obliterate`, `destroy`
  - Optional confirmation prompt with `--confirm` (aliases: `--padawan`, `--weakling`, `--jedi`)

- **`git sith help`** - Show help for commands (workaround for `git sith --help` being intercepted)

### Enhancements

- **Ignore command improvements:**
  - Template aliases (e.g., `csharp` → Dotnet, `js` → Node, `py` → Python)
  - `--aliases` flag to show all available aliases
  - Idempotent operation - won't add templates that already exist
  - "Dark side mode" - running without arguments creates a .gitignore that ignores everything
  - Works correctly from any subdirectory in the repository

- **Subdirectory support** - All commands now work correctly from any folder within the repository

- **RollForward enabled** - Tool now works with any .NET 8+ runtime version

## Version 0.0.1

Initial release featuring the `git sith ignore` command for easily adding .gitignore templates from GitHub's official collection. Supports single and multiple templates, with listing functionality.
