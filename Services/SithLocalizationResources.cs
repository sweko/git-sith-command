using System.CommandLine;
using System.CommandLine.Parsing;

namespace GitSith.Services;

/// <summary>
/// Custom localization resources that provide Sith-themed error messages
/// for System.CommandLine parsing errors.
/// </summary>
public class SithLocalizationResources : LocalizationResources
{
    /// <inheritdoc />
    public override string RequiredArgumentMissing(SymbolResult symbolResult)
    {
        var name = symbolResult.Symbol.Name;
        return $"Your lack of arguments disturbs me. '{name}' mandates an argument.";
    }

    /// <inheritdoc />
    public override string RequiredCommandWasNotProvided()
    {
        return "You have failed me for the last time. A directive is required.";
    }

    /// <inheritdoc />
    public override string UnrecognizedCommandOrArgument(string token)
    {
        return $"I find your lack of command knowledge disturbing. Your '{token}' is inadmissible.";
    }

    /// <inheritdoc />
    public override string FileDoesNotExist(string filePath)
    {
        return $"Perhaps the archives are incomplete. We have searched for the '{filePath}' file and been left wanting.";
    }

    /// <inheritdoc />
    public override string DirectoryDoesNotExist(string path)
    {
        return $"If it's not in the archives, it doesn't exist. Our search for the '{path}' folder has proven fruitless.";
    }
}
