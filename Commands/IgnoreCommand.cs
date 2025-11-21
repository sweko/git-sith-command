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
            description: "One or more template names to add (e.g., node, python, visualstudio)")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

        // Option to list available templates
        var listOption = new Option<bool>(["--list", "-l"], "List all available templates");

        command.AddArgument(templatesArgument);
        command.AddOption(listOption);

        command.SetHandler(async (templates, list) =>
        {
            var service = new GitIgnoreService();

            if (list)
            {
                await ListTemplatesAsync(service);
            }
            else if (templates.Length > 0)
            {
                await AddTemplatesAsync(service, templates);
            }
            else
            {
                Console.WriteLine("Usage: git sith ignore <template> [<template2> ...]");
                Console.WriteLine("       git sith ignore --list");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  git sith ignore node");
                Console.WriteLine("  git sith ignore node python");
                Console.WriteLine("  git sith ignore --list");
            }
        }, templatesArgument, listOption);

        return command;
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

        // Check if we're in a git repository
        if (!Directory.Exists(".git"))
        {
            Console.WriteLine("Error: Not a git repository. Run 'git init' first.");
            return;
        }

        Console.WriteLine($"Adding templates to {gitignorePath}...");

        var contents = new List<string>();
        var failedTemplates = new List<string>();

        foreach (var template in templates)
        {
            Console.Write($"  Fetching '{template}'... ");
            
            var content = await service.GetTemplateContentAsync(template);
            
            if (content != null)
            {
                contents.Add($"\n### {template} ###\n{content}");
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
}
