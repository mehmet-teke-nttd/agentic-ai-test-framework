using NUnit.Framework;
using Reqnroll;
using TestFramework.Playwright;

namespace UI.Tests.Steps.Demo;

[Binding]
public sealed class DemoTextFieldsSteps(ScenarioContext scenarioContext)
{
    private PlaywrightScenarioSession? _session;

    private PlaywrightScenarioSession RequireSession() =>
        _session ?? throw new InvalidOperationException("Browser session was not initialized.");

    private async Task EnsureSessionAsync()
    {
        if (_session is not null) return;

        _session = await PlaywrightScenarioSession.TryStartAsync();
        if (_session is null)
        {
            Assert.Ignore("Chromium is unavailable. Install it with: pwsh tests/UI.Tests/bin/Debug/net8.0/playwright.ps1 install chromium");
        }
    }

    private async Task NavigateToRegistrationAsync()
    {
        await EnsureSessionAsync();
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var applicationUrl = ApplicationUrlResolver.Load(settingsPath);
        await RequireSession().Page.GotoAsync($"{applicationUrl}/ui/textcaret-1");
    }

    [Then("the entered values should be visible in the respective text fields")]
    public async Task ThenTheEnteredValuesShouldBeVisibleInTheRespectiveTextFields()
    {
        var page = RequireSession().Page;
        var name = await page.InputValueAsync("input[placeholder='Enter your name']");
        var email = await page.InputValueAsync("input[placeholder='Enter Your Email']");
        var password = await page.InputValueAsync("input[placeholder='Enter your password']");

        Assert.That(name, Is.EqualTo("Jane Doe"));
        Assert.That(email, Is.EqualTo("jane.doe@example.com"));
        Assert.That(password, Is.EqualTo("SecurePass123!"));
    }

    [When("I view the text fields")]
    public Task WhenIViewTheTextFields() => Task.CompletedTask;

    [Then("each field should display its placeholder text")]
    public async Task ThenEachFieldShouldDisplayItsPlaceholderText()
    {
        var page = RequireSession().Page;
        var namePlaceholder = await page.GetAttributeAsync("input[placeholder='Enter your name']", "placeholder");
        var emailPlaceholder = await page.GetAttributeAsync("input[placeholder='Enter Your Email']", "placeholder");
        var passwordPlaceholder = await page.GetAttributeAsync("input[placeholder='Enter your password']", "placeholder");

        Assert.That(namePlaceholder, Is.EqualTo("Enter your name"));
        Assert.That(emailPlaceholder, Is.EqualTo("Enter Your Email"));
        Assert.That(passwordPlaceholder, Is.EqualTo("Enter your password"));
    }

    [Given("I am on the registration page with a field that has a default value")]
    public async Task GivenIAmOnTheRegistrationPageWithAFieldThatHasADefaultValue()
    {
        await NavigateToRegistrationAsync();
    }

    [When("I view the default value field")]
    public Task WhenIViewTheDefaultValueField() => Task.CompletedTask;

    [Then("the field should contain the pre-populated default value")]
    public async Task ThenTheFieldShouldContainThePrePopulatedDefaultValue()
    {
        var page = RequireSession().Page;
        var inputs = page.Locator("input[type='text'], input:not([type])");
        var count = await inputs.CountAsync();

        var foundDefault = false;
        for (var i = 0; i < count; i++)
        {
            var value = await inputs.Nth(i).InputValueAsync();
            if (!string.IsNullOrWhiteSpace(value))
            {
                foundDefault = true;
                break;
            }
        }

        Assert.That(foundDefault, Is.True, "Expected at least one field with a pre-populated default value.");
    }

    [When("I enter {string} into the name field")]
    public async Task WhenIEnterIntoTheNameField(string value)
    {
        await RequireSession().Page.FillAsync("input[placeholder='Enter your name']", value);
        scenarioContext["CapturedNameInput"] = value;
    }

    [When("I read back the name field value")]
    public async Task WhenIReadBackTheNameFieldValue()
    {
        var value = await RequireSession().Page.InputValueAsync("input[placeholder='Enter your name']");
        scenarioContext["CapturedNameValue"] = value;
    }

    [Then("the captured value should match {string}")]
    public void ThenTheCapturedValueShouldMatch(string expected)
    {
        var actual = scenarioContext.Get<string>("CapturedNameValue");
        Assert.That(actual, Is.EqualTo(expected));
    }

    [When("I leave required fields empty")]
    public Task WhenILeaveRequiredFieldsEmpty()
    {
        scenarioContext["FieldsEmpty"] = true;
        return Task.CompletedTask;
    }

    [Then("validation should prevent submission or show an error")]
    public async Task ThenValidationShouldPreventSubmissionOrShowAnError()
    {
        var page = RequireSession().Page;
        var urlBefore = page.Url;

        await page.ClickAsync("button:has-text('Register')");
        await Task.Delay(500);

        var hasValidationError = await page.Locator(".error-message, .validation-error, [class*='error'], :invalid").CountAsync() > 0;
        var stillOnPage = page.Url.Contains("/ui", StringComparison.OrdinalIgnoreCase);

        Assert.That(hasValidationError || stillOnPage, Is.True,
            "Expected validation error or to remain on registration page.");
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
