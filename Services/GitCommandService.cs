using System.Diagnostics;

namespace GitSith.Services;

public class GitCommandResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int ExitCode { get; set; }
}

public static class GitCommandService
{
    public static async Task<GitCommandResult> RunGitCommandAsync(params string[] args)
    {
        return await RunCommandAsync("git", null, args);
    }

    public static async Task<GitCommandResult> RunGitCommandInDirAsync(string workingDirectory, params string[] args)
    {
        return await RunCommandAsync("git", workingDirectory, args);
    }

    public static async Task<bool> CheckCommandExistsAsync(string command)
    {
        try
        {
            var result = await RunCommandAsync(command, null, "--version");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<GitCommandResult> RunCommandAsync(string command, string? workingDirectory, params string[] args)
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

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return new GitCommandResult
        {
            Success = process.ExitCode == 0,
            Output = output,
            Error = error,
            ExitCode = process.ExitCode
        };
    }
}
