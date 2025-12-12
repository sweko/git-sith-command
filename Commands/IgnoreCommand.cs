using System.CommandLine;
using GitSith.Services;

namespace GitSith.Commands;

public static class IgnoreCommand
{
    public static Command Create()
    {
        var command = new Command("ignore", "Manage .gitignore templates");

        // Argument for template names
        var templatesArgument = new Argument<string[]>(
            name: "templates",
            description: "One or more template names to add (e.g., node, python, csharp)")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        // Option to list available templates
        var listOption = new Option<bool>(["--list", "-l"], "List all available templates from GitHub");
        
        // Option to list aliases
        var aliasesOption = new Option<bool>(["--aliases", "-a"], "List all available template aliases");

        command.AddArgument(templatesArgument);
        command.AddOption(listOption);
        command.AddOption(aliasesOption);

        command.SetHandler(async (templates, list, aliases) =>
        {
            var service = new GitIgnoreService();

            if (aliases)
            {
                ListAliases(service);
            }
            else if (list)
            {
                await ListTemplatesAsync(service);
            }
            else if (templates.Length > 0)
            {
                await AddTemplatesAsync(service, templates);
            }
            else
            {
                await IgnoreEverythingAsync();
            }
        }, templatesArgument, listOption, aliasesOption);

        return command;
    }

    private static async Task IgnoreEverythingAsync()
    {
        const string gitignorePath = ".gitignore";
        
        var content = """
            # The dark side of .gitignore
            # "Everything that has transpired has done so according to my design."
            #   - Emperor Palpatine
            #
            # This repository has embraced the dark side.
            # All files are ignored. There is no hope. Only the void remains.

            # Ignore everything
            *

            # But not .gitignore itself (even the Sith have rules)
            !.gitignore
            """;

        await File.AppendAllTextAsync(gitignorePath, content + Environment.NewLine);
        
        Console.WriteLine("The dark side clouds everything...");
        Console.WriteLine($"✓ Created {gitignorePath} that ignores all files");
        Console.WriteLine();
        Console.WriteLine("\"Your feeble skills are no match for the power of the dark side.\"");
    }

    private static void ListAliases(GitIgnoreService service)
    {
        Console.WriteLine("Available template aliases:");
        Console.WriteLine(new string('-', 50));
        
        // Group aliases by their resolved template
        var grouped = service.GetAliasesGroupedByTemplate();

        foreach (var group in grouped)
        {
            var aliases = string.Join(", ", group.Select(kvp => kvp.Key).OrderBy(k => k));
            Console.WriteLine($"  {group.Key,-20} ← {aliases}");
        }
    }

    private static async Task ListTemplatesAsync(GitIgnoreService service)
    {
        Console.WriteLine("Fetching available templates...");
        var templates = await service.GetAvailableTemplatesAsync();

        if (templates.Count == 0)
        {
            Console.WriteLine("Could not fetch templates.");
            return;
        }

        Console.WriteLine($"\nAvailable templates ({templates.Count}):");
        Console.WriteLine(new string('-', 70));

        // Display in columns
        const int columnWidth = 25;
        const int columnsPerRow = 3;

        for (int i = 0; i < templates.Count; i++)
        {
            Console.Write(templates[i].PadRight(columnWidth));

            if ((i + 1) % columnsPerRow == 0)
            {
                Console.WriteLine();
            }
        }

        if (templates.Count % columnsPerRow != 0)
        {
            Console.WriteLine();
        }
    }

    private static async Task AddTemplatesAsync(GitIgnoreService service, string[] templates)
    {
        const string gitignorePath = ".gitignore";

        // Check if we're in a git repository (works from any subdirectory)
        var repoCheck = await RunGitCommandAsync("rev-parse", "--is-inside-work-tree");
        if (!repoCheck.Success || repoCheck.Output.Trim() != "true")
        {
            Console.WriteLine("Error: Not in a git repository. Run 'git init' first.");
            return;
        }

        Console.WriteLine($"Adding templates to {gitignorePath}...");

        var contents = new List<string>();
        var failedTemplates = new List<string>();

        foreach (var template in templates)
        {
            var resolvedTemplate = service.ResolveTemplateName(template);
            var displayName = template.Equals(resolvedTemplate, StringComparison.OrdinalIgnoreCase) 
                ? template 
                : $"{template} → {resolvedTemplate}";
            
            Console.Write($"  Fetching '{displayName}'... ");
            
            var content = await service.GetTemplateContentAsync(resolvedTemplate);
            
            if (content != null)
            {
                contents.Add($"\n### {resolvedTemplate} ###\n{content}");
                Console.WriteLine("✓");
            }
            else
            {
                failedTemplates.Add(template);
                Console.WriteLine("✗");
            }
        }

        if (contents.Count > 0)
        {
            // Append to .gitignore
            var fullContent = string.Join("\n", contents);
            await File.AppendAllTextAsync(gitignorePath, fullContent);

            Console.WriteLine($"\n✓ Successfully added {contents.Count} template(s) to {gitignorePath}");
        }

        if (failedTemplates.Count > 0)
        {
            Console.WriteLine($"\n✗ Failed to fetch: {string.Join(", ", failedTemplates)}");
            Console.WriteLine("  Run 'git sith ignore --list' to see available templates.");
        }
    }

    private static async Task<GitCommandResult> RunGitCommandAsync(params string[] args)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
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

        using var process = new System.Diagnostics.Process { StartInfo = startInfo };
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
