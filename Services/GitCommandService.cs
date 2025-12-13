using System.Diagnostics;

namespace GitSith.Services;

public record GitCommandResult(
    bool Success,
    string Output,
    string Error,
    int ExitCode
);

public static class GitCommandService
{
    public static async Task<GitCommandResult> RunGitCommandAsync(params string[] args)
    {
        return await RunCommandAsync("git", null, CancellationToken.None, args);
    }

    public static async Task<GitCommandResult> RunGitCommandAsync(CancellationToken cancellationToken, params string[] args)
    {
        return await RunCommandAsync("git", null, cancellationToken, args);
    }

    public static async Task<GitCommandResult> RunGitCommandInDirAsync(string workingDirectory, params string[] args)
    {
        return await RunCommandAsync("git", workingDirectory, CancellationToken.None, args);
    }

    public static async Task<GitCommandResult> RunGitCommandInDirAsync(string workingDirectory, CancellationToken cancellationToken, params string[] args)
    {
        return await RunCommandAsync("git", workingDirectory, cancellationToken, args);
    }

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
