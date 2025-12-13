using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;

namespace GitSith.Services;

/// <summary>
/// Extension methods for customizing the System.CommandLine help output.
/// </summary>
public static class HelpBuilderExtensions
{
    /// <summary>
    /// Configures the command line builder to display command aliases in help output.
    /// </summary>
    /// <param name="builder">The command line builder to configure.</param>
    /// <param name="rootCommand">The root command (aliases are not shown for the root).</param>
    /// <returns>The configured <see cref="CommandLineBuilder"/> for chaining.</returns>
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
