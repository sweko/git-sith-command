using System.CommandLine;
using System.CommandLine.Invocation;
using GitSith.Services;
using static GitSith.Services.GitCommandService;

namespace GitSith.Commands;

/// <summary>
/// Provides the ignore command for managing .gitignore templates.
/// Fetches templates from the GitHub API and supports template aliases.
/// </summary>
public static class IgnoreCommand
{
    /// <summary>
    /// Creates the ignore command with its arguments and options.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> for managing .gitignore templates.</returns>
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

        command.SetHandler(async (context) =>
        {
            var templates = context.ParseResult.GetValueForArgument(templatesArgument);
            var list = context.ParseResult.GetValueForOption(listOption);
            var aliases = context.ParseResult.GetValueForOption(aliasesOption);
            var cancellationToken = context.GetCancellationToken();

            using var service = new GitIgnoreService();

            if (aliases)
            {
                ListAliases(service);
            }
            else if (list)
            {
                await ListTemplatesAsync(service, cancellationToken);
            }
            else if (templates.Length > 0)
            {
                await AddTemplatesAsync(service, templates, cancellationToken);
            }
            else
            {
                await IgnoreEverythingAsync(cancellationToken);
            }
        });

        return command;
    }

    /// <summary>
    /// Creates a .gitignore file that ignores all files (the "dark side" option).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private static async Task IgnoreEverythingAsync(CancellationToken cancellationToken = default)
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

        await File.WriteAllTextAsync(gitignorePath, content + Environment.NewLine, cancellationToken);
        
        Console.WriteLine("The dark side clouds everything...");
        Console.WriteLine($"✓ Created {gitignorePath} that ignores all files");
        Console.WriteLine();
        Console.WriteLine("\"Your feeble skills are no match for the power of the dark side.\"");
    }

    /// <summary>
    /// Displays all available template aliases grouped by their resolved template name.
    /// </summary>
    /// <param name="service">The GitIgnore service instance.</param>
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

    /// <summary>
    /// Fetches and displays all available .gitignore templates from GitHub.
    /// </summary>
    /// <param name="service">The GitIgnore service instance.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private static async Task ListTemplatesAsync(GitIgnoreService service, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Fetching available templates...");
        var templates = await service.GetAvailableTemplatesAsync(cancellationToken);

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

    /// <summary>
    /// Fetches one or more templates and appends them to the .gitignore file.
    /// </summary>
    /// <param name="service">The GitIgnore service instance.</param>
    /// <param name="templates">The template names or aliases to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    private static async Task AddTemplatesAsync(GitIgnoreService service, string[] templates, CancellationToken cancellationToken = default)
    {
        const string gitignorePath = ".gitignore";

        // Check if we're in a git repository (works from any subdirectory)
        var repoCheck = await RunGitCommandAsync(cancellationToken, "rev-parse", "--is-inside-work-tree");
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
            
            var content = await service.GetTemplateContentAsync(resolvedTemplate, cancellationToken);
            
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
            await File.AppendAllTextAsync(gitignorePath, fullContent, cancellationToken);

            Console.WriteLine($"\n✓ Successfully added {contents.Count} template(s) to {gitignorePath}");
        }

        if (failedTemplates.Count > 0)
        {
            Console.WriteLine($"\n✗ Failed to fetch: {string.Join(", ", failedTemplates)}");
            Console.WriteLine("  Run 'git sith ignore --list' to see available templates.");
        }
    }
}
