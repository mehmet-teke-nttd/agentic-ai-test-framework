using System.Text.Json;
using Microsoft.Playwright;

namespace TestFramework.Playwright;

public sealed class PlaywrightScenarioSession : IAsyncDisposable
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IBrowserContext _context;
    private readonly List<object> _consoleMessages = [];

    private PlaywrightScenarioSession(
        IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage page)
    {
        _playwright = playwright;
        _browser = browser;
        _context = context;
        Page = page;
        Page.Console += (_, message) => _consoleMessages.Add(new
        {
            timestampUtc = DateTimeOffset.UtcNow,
            type = message.Type,
            text = message.Text
        });
    }

    public IPage Page { get; }

    public static async Task<PlaywrightScenarioSession?> TryStartAsync()
    {
        IPlaywright? playwright = null;
        try
        {
            playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            await context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
            var page = await context.NewPageAsync();
            return new PlaywrightScenarioSession(playwright, browser, context, page);
        }
        catch (PlaywrightException)
        {
            playwright?.Dispose();
            return null;
        }
    }

    public async Task CaptureFailureAsync(string scenarioName)
    {
        var directory = Environment.GetEnvironmentVariable("TEST_EVIDENCE_DIR");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        var safeName = string.Concat(scenarioName.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(directory, $"{safeName}.png"),
            FullPage = true
        });
        await _context.Tracing.StopAsync(new TracingStopOptions
        {
            Path = Path.Combine(directory, $"{safeName}-trace.zip")
        });
        await File.WriteAllTextAsync(
            Path.Combine(directory, $"{safeName}-browser-console.json"),
            JsonSerializer.Serialize(_consoleMessages, new JsonSerializerOptions { WriteIndented = true }));
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}
