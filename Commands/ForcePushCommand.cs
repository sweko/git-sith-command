using System.CommandLine;
using System.Diagnostics;

namespace GitSith.Commands;

public static class ForcePushCommand
{
    private static readonly string[] SithMessages =
    [
        "Do it.",
        "Unlimited power!",
        "I have altered the code. Pray I don't alter it further.",
        "The dark side clouds everything.",
        "Good... Good...",
        "Peace is a lie, there is only passion.",
        "Everything proceeds as I have foreseen.",
        "Your lack of commit message disturbs me.",
        "WIP: The dark side is strong",
        "Execute Order 66"
    ];

    public static Command Create()
    {
        var command = new Command("force-push", "Stage all changes, commit with a message, and push to remote");
        command.AddAlias("force");
        command.AddAlias("push");

        // Argument for commit message - can be a single quoted string or multiple words
        var messageArgument = new Argument<string[]>(
            name: "message",
            description: "The commit message (optional - uses a random Sith quote if not provided)")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        command.AddArgument(messageArgument);

        command.SetHandler(async (messageParts) =>
        {
            await ExecuteForcePushAsync(messageParts);
        }, messageArgument);

        return command;
    }

    private static async Task ExecuteForcePushAsync(string[] messageParts)
    {
        // Combine all message parts into a single message, or pick a random Sith quote
        var message = string.Join(" ", messageParts);

        if (string.IsNullOrWhiteSpace(message))
        {
            var random = new Random();
            message = SithMessages[random.Next(SithMessages.Length)];
            Console.WriteLine($"No message provided. The dark side speaks: \"{message}\"");
            Console.WriteLine();
        }

        // Check if we're in a git repository
        var repoCheck = await RunGitCommandAsync("rev-parse", "--show-toplevel");
        if (!repoCheck.Success)
        {
            Console.WriteLine("Error: Not in a git repository.");
            return;
        }
        var repoRoot = repoCheck.Output.Trim();
        Console.WriteLine($"Repository root: {repoRoot}");

        // Step 1: git add .
        Console.WriteLine("Staging all changes...");
        var addResult = await RunGitCommandAsync("add", ".");
        if (!addResult.Success)
        {
            Console.WriteLine($"Error staging changes: {addResult.Error}");
            return;
        }
        Console.WriteLine("✓ Changes staged");

        // Step 2: git commit -m <message>
        Console.WriteLine($"Committing with message: \"{message}\"");
        var commitResult = await RunGitCommandAsync("commit", "-m", message);
        if (!commitResult.Success)
        {
            // Check if it's just "nothing to commit"
            if (commitResult.Output.Contains("nothing to commit") || commitResult.Error.Contains("nothing to commit"))
            {
                Console.WriteLine("✓ Nothing to commit, working tree clean");
            }
            else
            {
                Console.WriteLine($"Error committing: {commitResult.Error}");
                return;
            }
        }
        else
        {
            Console.WriteLine("✓ Changes committed");
        }

        // Step 3: git push
        Console.WriteLine("Pushing to remote...");
        var pushResult = await RunGitCommandAsync("push");
        if (!pushResult.Success)
        {
            Console.WriteLine($"Error pushing: {pushResult.Error}");
            return;
        }
        Console.WriteLine("✓ Pushed to remote");

        Console.WriteLine();
        Console.WriteLine("Force push completed successfully!");
    }

    private static async Task<GitCommandResult> RunGitCommandAsync(params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

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

    private class GitCommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }
}
