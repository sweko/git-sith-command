using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitSith.Services;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(GitIgnoreService.GitIgnoreTemplate))]
internal partial class GitIgnoreJsonContext : JsonSerializerContext
{
}

public class GitIgnoreService : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;
    private const string GitHubApiBase = "https://api.github.com/gitignore";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        TypeInfoResolver = GitIgnoreJsonContext.Default
    };

    public GitIgnoreService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "git-sith");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    }

    public string ResolveTemplateName(string input)
    {
        return TemplateAliases.TryGetValue(input, out var resolved) ? resolved : input;
    }

    public IEnumerable<IGrouping<string, KeyValuePair<string, string>>> GetAliasesGroupedByTemplate()
    {
        return TemplateAliases
            .GroupBy(kvp => kvp.Value)
            .OrderBy(g => g.Key);
    }

    public async Task<List<string>> GetAvailableTemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await _httpClient.GetFromJsonAsync($"{GitHubApiBase}/templates", GitIgnoreJsonContext.Default.ListString, cancellationToken);
            return templates ?? [];
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled.");
            return [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching templates: {ex.Message}");
            return [];
        }
    }

    public async Task<string?> GetTemplateContentAsync(string templateName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync(
                $"{GitHubApiBase}/templates/{templateName}", 
                GitIgnoreJsonContext.Default.GitIgnoreTemplate,
                cancellationToken);
            return response?.Source;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled.");
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine($"Template '{templateName}' not found.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching template '{templateName}': {ex.Message}");
            return null;
        }
    }

    public class GitIgnoreTemplate
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #region Template Aliases

    // Aliases for common template names that differ from GitHub's naming
    private static readonly Dictionary<string, string> TemplateAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // Lowercase versions of all GitHub templates
        ["actionscript"] = "Actionscript",
        ["ada"] = "Ada",
        ["adventuregamestudio"] = "AdventureGameStudio",
        ["agda"] = "Agda",
        ["al"] = "AL",
        ["android"] = "Android",
        ["angular"] = "Angular",
        ["appengine"] = "AppEngine",
        ["appceleratortitanium"] = "AppceleratorTitanium",
        ["archlinuxpackages"] = "ArchLinuxPackages",
        ["autotools"] = "Autotools",
        ["ballerina"] = "Ballerina",
        ["c"] = "C",
        ["cfwheels"] = "CFWheels",
        ["cmake"] = "CMake",
        ["cuda"] = "CUDA",
        ["cakephp"] = "CakePHP",
        ["chefcookbook"] = "ChefCookbook",
        ["clojure"] = "Clojure",
        ["codeigniter"] = "CodeIgniter",
        ["commonlisp"] = "CommonLisp",
        ["composer"] = "Composer",
        ["concrete5"] = "Concrete5",
        ["coq"] = "Coq",
        ["craftcms"] = "CraftCMS",
        ["d"] = "D",
        ["dm"] = "DM",
        ["dart"] = "Dart",
        ["delphi"] = "Delphi",
        ["drupal"] = "Drupal",
        ["episerver"] = "EPiServer",
        ["eagle"] = "Eagle",
        ["elisp"] = "Elisp",
        ["elixir"] = "Elixir",
        ["elm"] = "Elm",
        ["erlang"] = "Erlang",
        ["expressionengine"] = "ExpressionEngine",
        ["extjs"] = "ExtJs",
        ["fancy"] = "Fancy",
        ["finale"] = "Finale",
        ["firebase"] = "Firebase",
        ["flaxengine"] = "FlaxEngine",
        ["flutter"] = "Flutter",
        ["forcedotcom"] = "ForceDotCom",
        ["fortran"] = "Fortran",
        ["fuelphp"] = "FuelPHP",
        ["gwt"] = "GWT",
        ["gcov"] = "Gcov",
        ["gitbook"] = "GitBook",
        ["githubpages"] = "GitHubPages",
        ["gleam"] = "Gleam",
        ["go"] = "Go",
        ["godot"] = "Godot",
        ["gradle"] = "Gradle",
        ["grails"] = "Grails",
        ["hip"] = "HIP",
        ["haskell"] = "Haskell",
        ["haxe"] = "Haxe",
        ["iar"] = "IAR",
        ["igorpro"] = "IGORPro",
        ["idris"] = "Idris",
        ["jboss"] = "JBoss",
        ["jenkins_home"] = "JENKINS_HOME",
        ["java"] = "Java",
        ["jekyll"] = "Jekyll",
        ["joomla"] = "Joomla",
        ["julia"] = "Julia",
        ["katalon"] = "Katalon",
        ["kicad"] = "KiCad",
        ["kohana"] = "Kohana",
        ["kotlin"] = "Kotlin",
        ["labview"] = "LabVIEW",
        ["langchain"] = "LangChain",
        ["laravel"] = "Laravel",
        ["leiningen"] = "Leiningen",
        ["lemonstand"] = "LemonStand",
        ["lilypond"] = "Lilypond",
        ["lithium"] = "Lithium",
        ["lua"] = "Lua",
        ["luau"] = "Luau",
        ["magento"] = "Magento",
        ["maven"] = "Maven",
        ["mercury"] = "Mercury",
        ["metaprogrammingsystem"] = "MetaProgrammingSystem",
        ["modelica"] = "Modelica",
        ["nanoc"] = "Nanoc",
        ["nestjs"] = "Nestjs",
        ["nextjs"] = "Nextjs",
        ["nim"] = "Nim",
        ["nix"] = "Nix",
        ["node"] = "Node",
        ["objective-c"] = "Objective-C",
        ["ocaml"] = "OCaml",
        ["opa"] = "Opa",
        ["opencart"] = "OpenCart",
        ["oracleforms"] = "OracleForms",
        ["packer"] = "Packer",
        ["perl"] = "Perl",
        ["phalcon"] = "Phalcon",
        ["playframework"] = "PlayFramework",
        ["plone"] = "Plone",
        ["prestashop"] = "Prestashop",
        ["processing"] = "Processing",
        ["purescript"] = "PureScript",
        ["python"] = "Python",
        ["qooxdoo"] = "Qooxdoo",
        ["qt"] = "Qt",
        ["r"] = "R",
        ["ros"] = "ROS",
        ["racket"] = "Racket",
        ["rails"] = "Rails",
        ["raku"] = "Raku",
        ["rescript"] = "ReScript",
        ["rhodesrhomobile"] = "RhodesRhomobile",
        ["ruby"] = "Ruby",
        ["rust"] = "Rust",
        ["scons"] = "SCons",
        ["ssdt-sqlproj"] = "SSDT-sqlproj",
        ["sass"] = "Sass",
        ["scala"] = "Scala",
        ["scheme"] = "Scheme",
        ["scrivener"] = "Scrivener",
        ["sdcc"] = "Sdcc",
        ["seamgen"] = "SeamGen",
        ["sketchup"] = "SketchUp",
        ["smalltalk"] = "Smalltalk",
        ["solidity-remix"] = "Solidity-Remix",
        ["stella"] = "Stella",
        ["sugarcrm"] = "SugarCRM",
        ["swift"] = "Swift",
        ["symfony"] = "Symfony",
        ["symphonycms"] = "SymphonyCMS",
        ["terraform"] = "Terraform",
        ["testcomplete"] = "TestComplete",
        ["textpattern"] = "Textpattern",
        ["turbogears2"] = "TurboGears2",
        ["twincat3"] = "TwinCAT3",
        ["typo3"] = "Typo3",
        ["unity"] = "Unity",
        ["unrealengine"] = "UnrealEngine",
        ["vba"] = "VBA",
        ["vvvv"] = "VVVV",
        ["visualstudio"] = "VisualStudio",
        ["waf"] = "Waf",
        ["wordpress"] = "WordPress",
        ["xojo"] = "Xojo",
        ["yeoman"] = "Yeoman",
        ["yii"] = "Yii",
        ["zendframework"] = "ZendFramework",
        ["zephir"] = "Zephir",
        ["zig"] = "Zig",
        ["ecu.test"] = "ecu.test",
        
        // .NET / C#
        ["c#"] = "VisualStudio",
        ["csharp"] = "VisualStudio",
        [".net"] = "Dotnet",
        ["vs"] = "VisualStudio",
        
        // JavaScript ecosystem
        ["js"] = "Node",
        ["javascript"] = "Node",
        ["npm"] = "Node",
        ["ts"] = "Node",
        ["typescript"] = "Node",
        
        // Python
        ["py"] = "Python",
        ["django"] = "Python",
        ["flask"] = "Python",
        
        // Java ecosystem
        ["spring"] = "Java",
        
        // Other common aliases
        ["cpp"] = "C++",
        ["rb"] = "Ruby",
        ["rs"] = "Rust",
        ["golang"] = "Go",
        ["objc"] = "Objective-C",
        ["latex"] = "TeX",
        ["tex"] = "TeX",
    };

    #endregion
}
