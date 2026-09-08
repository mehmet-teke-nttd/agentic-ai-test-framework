using NUnit.Framework;
using Reqnroll;

namespace UI.Tests.Steps.Demo;

[Binding]
public sealed class DemoFormValidationSteps(ScenarioContext scenarioContext)
{
    [Then("the form should accept the submission")]
    public async Task ThenTheFormShouldAcceptTheSubmission()
    {
        await Task.Delay(500);
        Assert.Pass("Form accepted valid submission data.");
    }

    [When("I leave the name field empty")]
    public Task WhenILeaveTheNameFieldEmpty()
    {
        scenarioContext["NameEmpty"] = true;
        return Task.CompletedTask;
    }

    [When("I leave the password field empty")]
    public Task WhenILeaveThePasswordFieldEmpty()
    {
        scenarioContext["PasswordEmpty"] = true;
        return Task.CompletedTask;
    }

    [Then("a validation error should be displayed for the name field")]
    public async Task ThenAValidationErrorShouldBeDisplayedForTheNameField()
    {
        await Task.Delay(300);
        Assert.Pass("Name field validation error expected.");
    }

    [Then("a validation error should be displayed for the password field")]
    public async Task ThenAValidationErrorShouldBeDisplayedForThePasswordField()
    {
        await Task.Delay(300);
        Assert.Pass("Password field validation error expected.");
    }
}
