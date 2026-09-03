using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps;

[Binding]
public sealed class StaticPageSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    [Given("a Chromium browser is available")]
    public async Task GivenAChromiumBrowserIsAvailable()
    {
        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }
    }

    [When("I open the configured application")]
    public async Task WhenIOpenTheConfiguredApplication()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await RequireSession().Page.GotoAsync(applicationUrl);
    }

    [Then("the greeting is {string}")]
    public async Task ThenTheGreetingIs(string expected)
    {
        var actual = await RequireSession().Page.Locator("#greeting").TextContentAsync();
        Assert.That(actual, Is.EqualTo(expected));
    }

    [AfterScenario]
    public async Task CaptureEvidenceAndCloseAsync()
    {
        if (_session is null) return;
        if (scenarioContext.TestError is not null)
        {
            await _session.CaptureFailureAsync(scenarioContext.ScenarioInfo.Title);
        }
        await _session.DisposeAsync();
    }

    private PlaywrightScenarioSession RequireSession() =>
        _session ?? throw new InvalidOperationException("Browser session was not initialized.");
}
