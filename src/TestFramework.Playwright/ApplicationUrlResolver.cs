using System.Text.Json;

namespace TestFramework.Playwright;

public static class ApplicationUrlResolver
{
    public const string EnvironmentVariableName = "TEST_APP_BASE_URL";

    public static string DefaultBaseUrl { get; } =
        $"data:text/html,{Uri.EscapeDataString("<html><body><h1 id='greeting'>Agentic test framework</h1></body></html>")}";

    public static string Resolve(string? configuredBaseUrl, string? environmentBaseUrl = null)
    {
        var value = !string.IsNullOrWhiteSpace(environmentBaseUrl)
            ? environmentBaseUrl
            : !string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? configuredBaseUrl
                : DefaultBaseUrl;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps &&
             uri.Scheme != "data"))
        {
            throw new InvalidOperationException(
                $"Application BaseUrl must be an absolute HTTP, HTTPS, or data URL. Received: '{value}'.");
        }

        return value;
    }

    public static string Load(string settingsPath)
    {
        string? configuredBaseUrl = null;
        if (File.Exists(settingsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(settingsPath));
            if (document.RootElement.TryGetProperty("Application", out var application) &&
                application.TryGetProperty("BaseUrl", out var baseUrl))
            {
                configuredBaseUrl = baseUrl.GetString();
            }
        }

        return Resolve(
            configuredBaseUrl,
            Environment.GetEnvironmentVariable(EnvironmentVariableName));
    }
}
