using System.CommandLine;
using System.CommandLine.Invocation;
using static GitSith.Services.GitCommandService;

namespace GitSith.Commands;

/// <summary>
/// Provides the purge command for completely removing files from git history.
/// Uses git-filter-repo if available, otherwise falls back to git filter-branch.
/// </summary>
public static class PurgeCommand
{
    /// <summary>
    /// Creates the purge command with its arguments and options.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> for purging files from history.</returns>
    /// <remarks>
    /// Aliases: obliterate, destroy, 66, order-66, damnatio-memoriae, memory-hole
    /// </remarks>
    public static Command Create()
    {
        var command = new Command("purge", "Remove a file from git history completely - this is the nuclear option");
        command.AddAlias("obliterate");
        command.AddAlias("destroy");
        command.AddAlias("66");
        command.AddAlias("order-66");
        command.AddAlias("damnatio-memoriae");
        command.AddAlias("memory-hole");

        var fileArgument = new Argument<string>(
            name: "file",
            description: "The file path to purge from history");

        var confirmOption = new Option<bool>(
            ["--confirm", "-c", "--padawan", "--weakling", "--jedi"],
            "Show confirmation prompt before proceeding (for the weak-willed)");

        command.AddArgument(fileArgument);
        command.AddOption(confirmOption);

        command.SetHandler(async (context) =>
        {
            var file = context.ParseResult.GetValueForArgument(fileArgument);
            var confirm = context.ParseResult.GetValueForOption(confirmOption);
            var cancellationToken = context.GetCancellationToken();
            await ExecutePurgeAsync(file, confirm, cancellationToken);
        });

        return command;
    }

    /// <summary>
    /// Executes the file purge operation, removing a file from all git history.
    /// </summary>
    /// <param name="filePath">The path to the file to purge.</param>
    /// <param name="confirm">If <c>true</c>, prompts the user for confirmation before proceeding.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private static async Task ExecutePurgeAsync(string filePath, bool confirm, CancellationToken cancellationToken = default)
    {
        // Check if we're in a git repository and get the root
        var repoCheck = await RunGitCommandAsync(cancellationToken, "rev-parse", "--is-inside-work-tree");
        if (!repoCheck.Success || repoCheck.Output.Trim() != "true")
        {
            Console.WriteLine("Error: Not in a git repository.");
            return;
        }

        // Get the repository root directory
        var repoRootResult = await RunGitCommandAsync(cancellationToken, "rev-parse", "--show-toplevel");
        if (!repoRootResult.Success)
        {
            Console.WriteLine("Error: Could not determine repository root.");
            return;
        }
        var repoRoot = repoRootResult.Output.Trim();

        // Calculate the relative path from repo root
        var currentDir = Directory.GetCurrentDirectory();
        var relativePath = Path.GetRelativePath(repoRoot, Path.Combine(currentDir, filePath));
        // Normalize to forward slashes for git
        relativePath = relativePath.Replace("\\", "/");

        // Check if git-filter-repo is available (preferred method)
        var hasFilterRepo = await CheckCommandExistsAsync("git-filter-repo", cancellationToken);

        Console.WriteLine();
        Console.WriteLine($"Purging '{relativePath}' from history...");
        Console.WriteLine($"Method: {(hasFilterRepo ? "git-filter-repo" : "git filter-branch")}");
        Console.WriteLine();

        if (confirm)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  You have chosen the path of hesitation.                     ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  This command will:                                          ║");
            Console.WriteLine("║  • Remove the file from ALL commits in history               ║");
            Console.WriteLine("║  • Rewrite the entire git history                            ║");
            Console.WriteLine("║  • Require a force push to update remotes                    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.Write("Type 'DO IT' to confirm: ");
            var confirmation = Console.ReadLine();
            
            if (confirmation != "DO IT")
            {
                Console.WriteLine();
                Console.WriteLine("\"You have failed me for the last time.\"");
                Console.WriteLine("Operation cancelled.");
                return;
            }
            Console.WriteLine();
        }

        Console.WriteLine("\"Execute Order 66...\"");
        Console.WriteLine();

        bool success;
        if (hasFilterRepo)
        {
            success = await PurgeWithFilterRepoAsync(relativePath, repoRoot, cancellationToken);
        }
        else
        {
            success = await PurgeWithFilterBranchAsync(relativePath, repoRoot, cancellationToken);
        }

        if (success)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  ✓ The file has been purged from history                     ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  Next steps:                                                 ║");
            Console.WriteLine("║  1. Verify the changes with: git log --all -- <filepath>     ║");
            Console.WriteLine("║  2. Force push to remote: git push --force --all             ║");
            Console.WriteLine("║  3. Tell collaborators to re-clone the repository            ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║  \"The circle is now complete.\"                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("\"I find your lack of faith disturbing.\"");
            Console.WriteLine("The purge operation failed. Check the error messages above.");
        }
    }

    /// <summary>
    /// Purges a file from history using the git-filter-repo tool (preferred method).
    /// </summary>
    /// <param name="filePath">The repository-relative path to purge.</param>
    /// <param name="repoRoot">The absolute path to the repository root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the purge succeeded; otherwise, <c>false</c>.</returns>
    private static async Task<bool> PurgeWithFilterRepoAsync(string filePath, string repoRoot, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Using git-filter-repo to purge file...");
        Console.WriteLine();

        var result = await RunGitCommandInDirAsync(repoRoot, cancellationToken, "filter-repo", "--invert-paths", "--path", filePath, "--force");

        if (!result.Success)
        {
            Console.WriteLine($"Error: {result.Error}");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(result.Output))
        {
            Console.WriteLine(result.Output);
        }

        return true;
    }

    /// <summary>
    /// Purges a file from history using git filter-branch (fallback method).
    /// </summary>
    /// <param name="filePath">The repository-relative path to purge.</param>
    /// <param name="repoRoot">The absolute path to the repository root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the purge succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is slower than git-filter-repo and is used as a fallback
    /// when git-filter-repo is not installed.
    /// </remarks>
    private static async Task<bool> PurgeWithFilterBranchAsync(string filePath, string repoRoot, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Using git filter-branch to purge file...");
        Console.WriteLine("(Consider installing git-filter-repo for better performance)");
        Console.WriteLine();

        // Path is already normalized with forward slashes
        var result = await RunGitCommandInDirAsync(
            repoRoot,
            cancellationToken,
            "filter-branch",
            "--force",
            "--index-filter",
            $"git rm --cached --ignore-unmatch \"{filePath}\"",
            "--prune-empty",
            "--tag-name-filter", "cat",
            "--", "--all");

        if (!result.Success)
        {
            // filter-branch outputs to stderr even on success sometimes
            if (result.Error.Contains("Ref 'refs/heads/") || result.Error.Contains("WARNING"))
            {
                Console.WriteLine(result.Error);
                return true;
            }
            Console.WriteLine($"Error: {result.Error}");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(result.Output))
        {
            Console.WriteLine(result.Output);
        }

        // Clean up the backup refs created by filter-branch
        Console.WriteLine("Cleaning up backup refs...");
        await RunGitCommandInDirAsync(repoRoot, cancellationToken, "for-each-ref", "--format=%(refname)", "refs/original/", 
            "|", "xargs", "-n", "1", "git", "update-ref", "-d");
        
        // Force garbage collection
        Console.WriteLine("Running garbage collection...");
        await RunGitCommandInDirAsync(repoRoot, cancellationToken, "reflog", "expire", "--expire=now", "--all");
        await RunGitCommandInDirAsync(repoRoot, cancellationToken, "gc", "--prune=now", "--aggressive");

        return true;
    }
}
