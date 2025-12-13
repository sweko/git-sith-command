using System.Diagnostics;

namespace GitSith.Services;

/// <summary>
/// Represents the result of executing a git command.
/// </summary>
/// <param name="Success">Indicates whether the command completed successfully (exit code 0).</param>
/// <param name="Output">The standard output from the command.</param>
/// <param name="Error">The standard error from the command.</param>
/// <param name="ExitCode">The process exit code.</param>
public record GitCommandResult(
    bool Success,
    string Output,
    string Error,
    int ExitCode
);

/// <summary>
/// Provides static methods for executing git commands and other external processes.
/// </summary>
public static class GitCommandService
{
    /// <summary>
    /// Runs a git command with the specified arguments.
    /// </summary>
    /// <param name="args">The arguments to pass to git.</param>
    /// <returns>A <see cref="GitCommandResult"/> containing the command output and status.</returns>
    public static async Task<GitCommandResult> RunGitCommandAsync(params string[] args)
    {
        return await RunCommandAsync("git", null, CancellationToken.None, args);
    }

    /// <summary>
    /// Runs a git command with the specified arguments and cancellation support.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="args">The arguments to pass to git.</param>
    /// <returns>A <see cref="GitCommandResult"/> containing the command output and status.</returns>
    public static async Task<GitCommandResult> RunGitCommandAsync(CancellationToken cancellationToken, params string[] args)
    {
        return await RunCommandAsync("git", null, cancellationToken, args);
    }

    /// <summary>
    /// Runs a git command in a specific directory.
    /// </summary>
    /// <param name="workingDirectory">The directory to run the command in.</param>
    /// <param name="args">The arguments to pass to git.</param>
    /// <returns>A <see cref="GitCommandResult"/> containing the command output and status.</returns>
    public static async Task<GitCommandResult> RunGitCommandInDirAsync(string workingDirectory, params string[] args)
    {
        return await RunCommandAsync("git", workingDirectory, CancellationToken.None, args);
    }

    /// <summary>
    /// Runs a git command in a specific directory with cancellation support.
    /// </summary>
    /// <param name="workingDirectory">The directory to run the command in.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="args">The arguments to pass to git.</param>
    /// <returns>A <see cref="GitCommandResult"/> containing the command output and status.</returns>
    public static async Task<GitCommandResult> RunGitCommandInDirAsync(string workingDirectory, CancellationToken cancellationToken, params string[] args)
    {
        return await RunCommandAsync("git", workingDirectory, cancellationToken, args);
    }

    /// <summary>
    /// Checks whether an external command is available on the system.
    /// </summary>
    /// <param name="command">The command name to check (e.g., "git-filter-repo").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the command exists and responds to --version; otherwise, <c>false</c>.</returns>
    public static async Task<bool> CheckCommandExistsAsync(string command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RunCommandAsync(command, null, cancellationToken, "--version");
            return result.Success;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Runs an external command with the specified arguments.
    /// </summary>
    /// <param name="command">The command or executable to run.</param>
    /// <param name="workingDirectory">The working directory, or <c>null</c> for the current directory.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="args">The arguments to pass to the command.</param>
    /// <returns>A <see cref="GitCommandResult"/> containing the command output and status.</returns>
    /// <remarks>
    /// If cancelled, the process tree is killed before the exception propagates.
    /// </remarks>
    public static async Task<GitCommandResult> RunCommandAsync(string command, string? workingDirectory, CancellationToken cancellationToken, params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* best effort */ }
            throw;
        }

        var output = await outputTask;
        var error = await errorTask;

        return new GitCommandResult(
            Success: process.ExitCode == 0,
            Output: output,
            Error: error,
            ExitCode: process.ExitCode
        );
    }
}
