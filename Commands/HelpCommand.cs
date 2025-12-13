using System.CommandLine;
using System.CommandLine.Invocation;

namespace GitSith.Commands;

/// <summary>
/// Provides the help command for displaying usage information about git-sith commands.
/// </summary>
public static class HelpCommand
{
    /// <summary>
    /// Creates the help command that displays usage information.
    /// </summary>
    /// <param name="rootCommand">The root command to delegate help requests to.</param>
    /// <returns>A configured <see cref="Command"/> for displaying help.</returns>
    public static Command Create(RootCommand rootCommand)
    {
        var commandArgument = new Argument<string?>(
            name: "command",
            description: "The command to get help for",
            getDefaultValue: () => null);

        var command = new Command("help", "Show help for git-sith commands")
        {
            commandArgument
        };

        command.SetHandler(async (context) =>
        {
            var commandName = context.ParseResult.GetValueForArgument(commandArgument);
            
            if (string.IsNullOrEmpty(commandName))
            {
                // Show general help
                await rootCommand.InvokeAsync(["--help"]);
            }
            else
            {
                // Show help for specific command
                await rootCommand.InvokeAsync([commandName, "--help"]);
            }
        });

        return command;
    }
}
