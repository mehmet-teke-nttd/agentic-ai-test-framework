using NUnit.Framework;
using Reqnroll;
using UI.Tests.PageObjects;
using TestFramework.Playwright;

namespace UI.Tests.Steps.Demo;

[Binding]
public sealed class DemoCheckboxesSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;
    private NotificationPreferencesPage? _notificationPreferencesPage;

    private static readonly string[] AllCheckboxLabels =
    [
        "Email", "WhatsApp", "Message", "Yahoo",
        "Sandals", "Shoes", "Flipper", "No thanks",
        "Regarding the same product", "Regarding similar products"
    ];

    private PlaywrightScenarioSession RequireSession() =>
        _session ?? throw new InvalidOperationException("Browser session was not initialized.");

    private NotificationPreferencesPage RequireNotificationPreferencesPage() =>
        _notificationPreferencesPage ?? throw new InvalidOperationException("Notification preferences page object was not initialized.");

    private async Task EnsureSessionOnCheckboxPageAsync()
    {
        if (_session is not null) return;

        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        _notificationPreferencesPage = new NotificationPreferencesPage(_session.Page);
        await _notificationPreferencesPage.NavigateAsync(applicationUrl);
    }

    [Given("I am on the demo checkbox page")]
    public async Task GivenIAmOnTheDemoCheckboxPage()
    {
        await EnsureSessionOnCheckboxPageAsync();
    }

    [When("I check the demo {string} notification checkbox")]
    public async Task WhenICheckTheDemoNotificationCheckbox(string label)
    {
        await EnsureSessionOnCheckboxPageAsync();
        await RequireNotificationPreferencesPage().CheckByLabelAsync(label);
    }

    [When("I check the demo {string} recommendation checkbox")]
    public async Task WhenICheckTheDemoRecommendationCheckbox(string label)
    {
        await EnsureSessionOnCheckboxPageAsync();
        await RequireNotificationPreferencesPage().CheckByLabelAsync(label);
    }

    [Then("the demo {string} checkbox should be checked")]
    public async Task ThenTheDemoCheckboxShouldBeChecked(string label)
    {
        var isChecked = await RequireNotificationPreferencesPage().IsCheckedByLabelAsync(label);
        Assert.That(isChecked, Is.True, $"Expected '{label}' checkbox to be checked.");
    }

    [When("I select all demo notification and recommendation checkboxes")]
    [Given("I have selected all demo notification and recommendation checkboxes")]
    public async Task WhenISelectAllDemoNotificationAndRecommendationCheckboxes()
    {
        await EnsureSessionOnCheckboxPageAsync();
        await RequireNotificationPreferencesPage().CheckAllByLabelsAsync(AllCheckboxLabels);
    }

    [Then("all demo checkboxes should be checked")]
    public async Task ThenAllDemoCheckboxesShouldBeChecked()
    {
        var pageObject = RequireNotificationPreferencesPage();
        foreach (var label in AllCheckboxLabels)
        {
            Assert.That(await pageObject.IsCheckedByLabelAsync(label), Is.True,
                $"Expected '{label}' checkbox to be checked.");
        }
    }

    [When("I click the demo Continue button")]
    public async Task WhenIClickTheDemoContinueButton()
    {
        await RequireNotificationPreferencesPage().ClickContinueAsync();
        await Task.Delay(500);
    }

    [Then("the demo checkbox page should proceed without errors")]
    public async Task ThenTheDemoCheckboxPageShouldProceedWithoutErrors()
    {
        var hasError = await RequireNotificationPreferencesPage().HasVisibleErrorsAsync();
        Assert.That(hasError, Is.False, "Expected checkbox page to proceed without errors.");
    }

    [When("no demo checkboxes have been selected")]
    public Task WhenNoDemoCheckboxesHaveBeenSelected() => Task.CompletedTask;

    [Then("all demo checkboxes should be unchecked by default")]
    public async Task ThenAllDemoCheckboxesShouldBeUncheckedByDefault()
    {
        var pageObject = RequireNotificationPreferencesPage();
        foreach (var label in AllCheckboxLabels)
        {
            Assert.That(await pageObject.IsCheckedByLabelAsync(label), Is.False,
                $"Expected '{label}' checkbox to be unchecked by default.");
        }
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
}
