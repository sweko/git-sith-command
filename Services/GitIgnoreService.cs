using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitSith.Services;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(GitIgnoreService.GitIgnoreTemplate))]
internal partial class GitIgnoreJsonContext : JsonSerializerContext
{
}

public class GitIgnoreService
{
    private readonly HttpClient _httpClient;
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

    public async Task<List<string>> GetAvailableTemplatesAsync()
    {
        try
        {
            var templates = await _httpClient.GetFromJsonAsync($"{GitHubApiBase}/templates", GitIgnoreJsonContext.Default.ListString);
            return templates ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching templates: {ex.Message}");
            return [];
        }
    }

    public async Task<string?> GetTemplateContentAsync(string templateName)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync(
                $"{GitHubApiBase}/templates/{templateName}", 
                GitIgnoreJsonContext.Default.GitIgnoreTemplate);
            return response?.Source;
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
}
