using System.CommandLine;
using GitSith.Commands;

namespace GitSith;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("git-sith: A developer-oriented Git power tool");

        // Add the ignore command
        rootCommand.AddCommand(IgnoreCommand.Create());

        // Add the force-push command
        rootCommand.AddCommand(ForcePushCommand.Create());

        // Add the help command
        rootCommand.AddCommand(HelpCommand.Create(rootCommand));

        return await rootCommand.InvokeAsync(args);
    }
}
