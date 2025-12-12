using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using GitSith.Commands;
using GitSith.Services;

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

        // Add the purge command
        rootCommand.AddCommand(PurgeCommand.Create());

        // Add the help command
        rootCommand.AddCommand(HelpCommand.Create(rootCommand));

        var parser = new CommandLineBuilder(rootCommand)
            .UseLocalizationResources(new SithLocalizationResources())
            .UseAliasHelp(rootCommand)
            .UseDefaults()
            .Build();

        return await parser.InvokeAsync(args);
    }
}
