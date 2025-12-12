using System.CommandLine;
using System.CommandLine.Parsing;

namespace GitSith.Services;

public class SithLocalizationResources : LocalizationResources
{
    public override string RequiredArgumentMissing(SymbolResult symbolResult)
    {
        var name = symbolResult.Symbol.Name;
        return $"Your lack of arguments disturbs me. '{name}' mandates an argument.";
    }

    public override string RequiredCommandWasNotProvided()
    {
        return "You have failed me for the last time. A directive is required.";
    }

    public override string UnrecognizedCommandOrArgument(string token)
    {
        return $"I find your lack of command knowledge disturbing. Your '{token}' is inadmissible.";
    }

    public override string FileDoesNotExist(string filePath)
    {
        return $"Perhaps the archives are incomplete. We have searched for the '{filePath}' file and been left wanting.";
    }

    public override string DirectoryDoesNotExist(string path)
    {
        return $"If it's not in the archives, it doesn't exist. Our search for the '{path}' folder has proven fruitless.";
    }
}
