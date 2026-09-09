using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;
using UI.Tests.PageObjects;

namespace UI.Tests.Steps.Demo;

/// <summary>
/// Step definitions for DemoApps Button Interaction feature
/// Uses Page Object Model pattern - all Playwright interactions are in DemoButtonInteractionPage
/// </summary>
[Binding]
public sealed class DemoButtonInteractionSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;
    private DemoButtonInteractionPage? _demoButtonInteractionPage;

    [Given("a Chromium browser is available")]
    public async Task GivenAChromiumBrowserIsAvailable()
    {
        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }
        
        // Initialize Page Object
        _demoButtonInteractionPage = new DemoButtonInteractionPage(RequireSession().Page);
    }

    [Given("I am on the DemoApps website")]
    public async Task GivenIAmOnTheDemoAppsWebsite()
    {
        if (_session is null)
        {
            await GivenAChromiumBrowserIsAvailable();
        }

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        
        // Ensure Page Object is initialized
        _demoButtonInteractionPage ??= new DemoButtonInteractionPage(RequireSession().Page);
        
        // Navigate using Page Object
        await RequireDemoButtonInteractionPage().NavigateToHomepageAsync(applicationUrl);
    }

    [Given("I am on the DemoApps homepage")]
    public async Task GivenIAmOnTheDemoAppsHomepage()
    {
        await GivenIAmOnTheDemoAppsWebsite();
    }

    [Given("I am on the Button testing page")]
    public async Task GivenIAmOnTheButtonTestingPage()
    {
        if (_session is null)
        {
            await GivenAChromiumBrowserIsAvailable();
        }

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        
        // Ensure Page Object is initialized
        _demoButtonInteractionPage ??= new DemoButtonInteractionPage(RequireSession().Page);
        
        // Navigate using Page Object
        // TODO: Update the route based on actual DemoApps URL structure
        await RequireDemoButtonInteractionPage().NavigateToButtonTestingPageAsync(applicationUrl);
    }

    [When("I navigate to {string}")]
    public async Task WhenINavigateTo(string section)
    {
        await RequireDemoButtonInteractionPage().NavigateToUITestingConceptsAsync();
    }

    [When("I select {string} from the left menu")]
    public async Task WhenISelectFromTheLeftMenu(string menuItem)
    {
        await RequireDemoButtonInteractionPage().SelectButtonFromMenuAsync();
    }

    [When("I click the {string} button")]
    public async Task WhenIClickTheButton(string buttonText)
    {
        await RequireDemoButtonInteractionPage().ClickYesButtonAsync();
    }

    [Then("the Button testing page should be displayed")]
    public async Task ThenTheButtonTestingPageShouldBeDisplayed()
    {
        var isDisplayed = await RequireDemoButtonInteractionPage().IsButtonPageDisplayedAsync();
        Assert.That(isDisplayed, Is.True, "Expected to be on Button testing page");
    }

    [Then("I should see the confirmation text {string}")]
    public async Task ThenIShouldSeeTheConfirmationText(string expectedText)
    {
        await RequireDemoButtonInteractionPage().VerifyConfirmationTextAsync(expectedText);
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

    private DemoButtonInteractionPage RequireDemoButtonInteractionPage() =>
        _demoButtonInteractionPage ?? throw new InvalidOperationException("DemoButtonInteractionPage was not initialized.");
}
