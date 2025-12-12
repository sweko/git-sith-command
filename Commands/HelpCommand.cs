using System.CommandLine;
using System.CommandLine.Invocation;

namespace GitSith.Commands;

public static class HelpCommand
{
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
