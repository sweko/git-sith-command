using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;

namespace GitSith.Services;

public static class HelpBuilderExtensions
{
    public static CommandLineBuilder UseAliasHelp(this CommandLineBuilder builder, RootCommand rootCommand)
    {
        return builder.UseHelp(ctx =>
        {
            ctx.HelpBuilder.CustomizeLayout(_ =>
                HelpBuilder.Default
                    .GetLayout()
                    .Prepend(helpContext =>
                    {
                        var cmd = helpContext.Command;
                        if (cmd != rootCommand)
                        {
                            // Show aliases
                            var aliases = cmd.Aliases.ToList();
                            if (aliases.Count > 1)
                            {
                                helpContext.Output.WriteLine();
                                helpContext.Output.WriteLine($"Aliases: {string.Join(", ", aliases)}");
                            }
                        }
                    }));
        });
    }
}
