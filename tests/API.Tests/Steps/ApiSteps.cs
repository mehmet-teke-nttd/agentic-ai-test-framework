using System.Diagnostics;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reqnroll;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Steps;

[Binding]
public class ApiSteps
{
    private readonly HttpClient _httpClient;
    private HttpResponseMessage? _response;
    private string _baseUrl;
    private readonly Dictionary<string, object> _testData;
    private readonly Stopwatch _stopwatch;
    private string _requestBody = string.Empty;

    public ApiSteps()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        _baseUrl = Environment.GetEnvironmentVariable("TEST_API_BASE_URL") 
            ?? configuration["Api:BaseUrl"] 
            ?? throw new InvalidOperationException("API Base URL is not configured");

        var timeout = int.TryParse(configuration["Api:Timeout"], out var timeoutSeconds) 
            ? timeoutSeconds 
            : 30;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(timeout)
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _testData = new Dictionary<string, object>();
        _stopwatch = new Stopwatch();
    }

    #region Given Steps

    [Given("the API is available at the configured base URL")]
    public void GivenTheApiIsAvailable()
    {
        Assert.That(_baseUrl, Is.Not.Null.And.Not.Empty, "Base URL should be configured");
    }

    [Given("I have the following user data:")]
    public void GivenIHaveUserData(Table table)
    {
        var userData = TableToDictionary(table);
        _testData["userData"] = userData;
        _requestBody = JsonConvert.SerializeObject(userData);
    }

    [Given("a user exists with id {string}")]
    public async Task GivenUserExists(string userId)
    {
        var response = await _httpClient.GetAsync($"/users/{userId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), 
            $"User with id {userId} should exist");
        _testData["existingUserId"] = userId;
    }

    [Given("I have updated user data:")]
    public void GivenIHaveUpdatedUserData(Table table)
    {
        var userData = TableToDictionary(table);
        _testData["updatedUserData"] = userData;
        _requestBody = JsonConvert.SerializeObject(userData);
    }

    [Given("I have the following post data:")]
    public void GivenIHavePostData(Table table)
    {
        var postData = TableToDictionary(table);
        _testData["postData"] = postData;
        _requestBody = JsonConvert.SerializeObject(postData);
    }

    [Given("a post exists with id {string}")]
    public async Task GivenPostExists(string postId)
    {
        var response = await _httpClient.GetAsync($"/posts/{postId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), 
            $"Post with id {postId} should exist");
        _testData["existingPostId"] = postId;
    }

    [Given("I have updated post data:")]
    public void GivenIHaveUpdatedPostData(Table table)
    {
        var postData = TableToDictionary(table);
        _testData["updatedPostData"] = postData;
        _requestBody = JsonConvert.SerializeObject(postData);
    }

    [Given("I have incomplete post data:")]
    public void GivenIHaveIncompletePostData(Table table)
    {
        var postData = TableToDictionary(table);
        _testData["incompletePostData"] = postData;
        _requestBody = JsonConvert.SerializeObject(postData);
    }

    #endregion

    #region When Steps - HTTP Methods

    [When("I send a GET request to {string}")]
    public async Task WhenISendGetRequest(string endpoint)
    {
        _stopwatch.Restart();
        _response = await _httpClient.GetAsync(endpoint);
        _stopwatch.Stop();
    }

    [When("I send a GET request to {string} with query parameter {string} equals {string}")]
    public async Task WhenISendGetRequestWithQueryParam(string endpoint, string parameter, string value)
    {
        var url = $"{endpoint}?{parameter}={Uri.EscapeDataString(value)}";
        _stopwatch.Restart();
        _response = await _httpClient.GetAsync(url);
        _stopwatch.Stop();
    }

    [When("I send a POST request to {string} with the user data")]
    [When("I send a POST request to {string} with the post data")]
    public async Task WhenISendPostRequest(string endpoint)
    {
        var content = new StringContent(_requestBody, Encoding.UTF8, "application/json");
        _stopwatch.Restart();
        _response = await _httpClient.PostAsync(endpoint, content);
        _stopwatch.Stop();
    }

    [When("I send a PUT request to {string} with the updated data")]
    public async Task WhenISendPutRequest(string endpoint)
    {
        var content = new StringContent(_requestBody, Encoding.UTF8, "application/json");
        _stopwatch.Restart();
        _response = await _httpClient.PutAsync(endpoint, content);
        _stopwatch.Stop();
    }

    [When("I send a PATCH request to {string} with:")]
    public async Task WhenISendPatchRequest(string endpoint, Table table)
    {
        var patchData = TableToDictionary(table);
        var content = new StringContent(
            JsonConvert.SerializeObject(patchData), 
            Encoding.UTF8, 
            "application/json");
        
        _stopwatch.Restart();
        _response = await _httpClient.PatchAsync(endpoint, content);
        _stopwatch.Stop();
    }

    [When("I send a DELETE request to {string}")]
    public async Task WhenISendDeleteRequest(string endpoint)
    {
        _stopwatch.Restart();
        _response = await _httpClient.DeleteAsync(endpoint);
        _stopwatch.Stop();
    }

    #endregion

    #region Then Steps - Status Codes

    [Then("the response status code should be {int}")]
    public void ThenResponseStatusCodeShouldBe(int expectedStatusCode)
    {
        Assert.That(_response, Is.Not.Null, "Response should not be null");
        Assert.That((int)_response!.StatusCode, Is.EqualTo(expectedStatusCode),
            $"Expected status code {expectedStatusCode} but got {(int)_response.StatusCode}");
    }

    [Then("the response status code should be {int} or {int}")]
    public void ThenResponseStatusCodeShouldBeOneOf(int statusCode1, int statusCode2)
    {
        Assert.That(_response, Is.Not.Null, "Response should not be null");
        var actualStatusCode = (int)_response!.StatusCode;
        Assert.That(actualStatusCode, Is.EqualTo(statusCode1).Or.EqualTo(statusCode2),
            $"Expected status code {statusCode1} or {statusCode2} but got {actualStatusCode}");
    }

    #endregion

    #region Then Steps - Response Content

    [Then("the response should contain a list of users")]
    public async Task ThenResponseShouldContainListOfUsers()
    {
        var content = await GetResponseContent();
        var users = JsonConvert.DeserializeObject<JArray>(content);
        Assert.That(users, Is.Not.Null, "Response should be a valid JSON array");
        Assert.That(users!.Count, Is.GreaterThan(0), "Users list should not be empty");
    }

    [Then("each user should have required fields {string}")]
    public async Task ThenEachUserShouldHaveRequiredFields(string fieldsString)
    {
        var content = await GetResponseContent();
        var users = JsonConvert.DeserializeObject<JArray>(content);
        var requiredFields = fieldsString.Split(',').Select(f => f.Trim()).ToArray();

        Assert.That(users, Is.Not.Null);
        foreach (var user in users!)
        {
            foreach (var field in requiredFields)
            {
                Assert.That(user[field], Is.Not.Null, 
                    $"User should have field '{field}'");
            }
        }
    }

    [Then("the response should contain a user with id {string}")]
    public async Task ThenResponseShouldContainUserWithId(string userId)
    {
        var content = await GetResponseContent();
        var user = JsonConvert.DeserializeObject<JObject>(content);
        Assert.That(user, Is.Not.Null);
        Assert.That(user!["id"]?.ToString(), Is.EqualTo(userId));
    }

    [Then("the user should have a valid email address")]
    public async Task ThenUserShouldHaveValidEmail()
    {
        var content = await GetResponseContent();
        var user = JsonConvert.DeserializeObject<JObject>(content);
        var email = user?["email"]?.ToString();
        
        Assert.That(email, Is.Not.Null.And.Not.Empty);
        Assert.That(email, Does.Contain("@"), "Email should contain @ symbol");
    }

    [Then("the response should contain the created user")]
    [Then("the response should contain the updated user")]
    public async Task ThenResponseShouldContainUser()
    {
        var content = await GetResponseContent();
        var user = JsonConvert.DeserializeObject<JObject>(content);
        Assert.That(user, Is.Not.Null, "Response should contain user data");
    }

    [Then("the created user should have an {string} field")]
    [Then("the post should have an {string} field")]
    public async Task ThenCreatedItemShouldHaveField(string fieldName)
    {
        var content = await GetResponseContent();
        var item = JsonConvert.DeserializeObject<JObject>(content);
        Assert.That(item?[fieldName], Is.Not.Null, 
            $"Created item should have '{fieldName}' field");
    }

    [Then("the user {string} should be {string}")]
    [Then("the post {string} should be {string}")]
    public async Task ThenFieldShouldHaveValue(string fieldName, string expectedValue)
    {
        var content = await GetResponseContent();
        var item = JsonConvert.DeserializeObject<JObject>(content);
        var actualValue = item?[fieldName]?.ToString();
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }

    [Then("the response should contain multiple posts")]
    public async Task ThenResponseShouldContainMultiplePosts()
    {
        var content = await GetResponseContent();
        var posts = JsonConvert.DeserializeObject<JArray>(content);
        Assert.That(posts, Is.Not.Null);
        Assert.That(posts!.Count, Is.GreaterThan(1), "Should contain multiple posts");
    }

    [Then("each post should have {string} fields")]
    public async Task ThenEachPostShouldHaveFields(string fieldsString)
    {
        var content = await GetResponseContent();
        var posts = JsonConvert.DeserializeObject<JArray>(content);
        var requiredFields = fieldsString.Split(',').Select(f => f.Trim()).ToArray();

        Assert.That(posts, Is.Not.Null);
        foreach (var post in posts!)
        {
            foreach (var field in requiredFields)
            {
                Assert.That(post[field], Is.Not.Null, 
                    $"Post should have field '{field}'");
            }
        }
    }

    [Then("all posts should belong to user {string}")]
    public async Task ThenAllPostsShouldBelongToUser(string userId)
    {
        var content = await GetResponseContent();
        var posts = JsonConvert.DeserializeObject<JArray>(content);
        
        Assert.That(posts, Is.Not.Null);
        foreach (var post in posts!)
        {
            Assert.That(post["userId"]?.ToString(), Is.EqualTo(userId));
        }
    }

    [Then("the post should have id {string}")]
    public async Task ThenPostShouldHaveId(string postId)
    {
        var content = await GetResponseContent();
        var post = JsonConvert.DeserializeObject<JObject>(content);
        Assert.That(post?["id"]?.ToString(), Is.EqualTo(postId));
    }

    [Then("the post should have a non-empty title and body")]
    public async Task ThenPostShouldHaveNonEmptyTitleAndBody()
    {
        var content = await GetResponseContent();
        var post = JsonConvert.DeserializeObject<JObject>(content);
        
        Assert.That(post?["title"]?.ToString(), Is.Not.Null.And.Not.Empty);
        Assert.That(post?["body"]?.ToString(), Is.Not.Null.And.Not.Empty);
    }

    [Then("the response should contain the created post")]
    public async Task ThenResponseShouldContainCreatedPost()
    {
        var content = await GetResponseContent();
        var post = JsonConvert.DeserializeObject<JObject>(content);
        Assert.That(post, Is.Not.Null, "Response should contain post data");
    }

    [Then("the response should contain comments")]
    public async Task ThenResponseShouldContainComments()
    {
        var content = await GetResponseContent();
        var comments = JsonConvert.DeserializeObject<JArray>(content);
        Assert.That(comments, Is.Not.Null);
        Assert.That(comments!.Count, Is.GreaterThan(0));
    }

    [Then("each comment should have {string} fields")]
    public async Task ThenEachCommentShouldHaveFields(string fieldsString)
    {
        var content = await GetResponseContent();
        var comments = JsonConvert.DeserializeObject<JArray>(content);
        var requiredFields = fieldsString.Split(',').Select(f => f.Trim()).ToArray();

        Assert.That(comments, Is.Not.Null);
        foreach (var comment in comments!)
        {
            foreach (var field in requiredFields)
            {
                Assert.That(comment[field], Is.Not.Null, 
                    $"Comment should have field '{field}'");
            }
        }
    }

    [Then("all comments should belong to post {string}")]
    public async Task ThenAllCommentsShouldBelongToPost(string postId)
    {
        var content = await GetResponseContent();
        var comments = JsonConvert.DeserializeObject<JArray>(content);
        
        Assert.That(comments, Is.Not.Null);
        foreach (var comment in comments!)
        {
            Assert.That(comment["postId"]?.ToString(), Is.EqualTo(postId));
        }
    }

    [Then("the post {string} should not be empty")]
    public async Task ThenPostFieldShouldNotBeEmpty(string fieldName)
    {
        var content = await GetResponseContent();
        var post = JsonConvert.DeserializeObject<JObject>(content);
        Assert.That(post?[fieldName]?.ToString(), Is.Not.Null.And.Not.Empty);
    }

    [Then("the post {string} should be greater than {int}")]
    public async Task ThenPostFieldShouldBeGreaterThan(string fieldName, int minValue)
    {
        var content = await GetResponseContent();
        var post = JsonConvert.DeserializeObject<JObject>(content);
        var value = post?[fieldName]?.Value<int>();
        Assert.That(value, Is.GreaterThan(minValue));
    }

    [Then("the response should be filtered by {string}")]
    public async Task ThenResponseShouldBeFiltered(string parameter)
    {
        var content = await GetResponseContent();
        var items = JsonConvert.DeserializeObject<JArray>(content);
        Assert.That(items, Is.Not.Null);
        Assert.That(items!.Count, Is.GreaterThan(0), "Filtered results should not be empty");
    }

    #endregion

    #region Then Steps - Performance & Headers

    [Then("the response should be received within {int} milliseconds")]
    public void ThenResponseShouldBeReceivedWithinTime(int maxMilliseconds)
    {
        Assert.That(_stopwatch.ElapsedMilliseconds, Is.LessThanOrEqualTo(maxMilliseconds),
            $"Response time {_stopwatch.ElapsedMilliseconds}ms exceeded maximum {maxMilliseconds}ms");
    }

    [Then("the response content type should be {string}")]
    public void ThenResponseContentTypeShouldBe(string expectedContentType)
    {
        Assert.That(_response, Is.Not.Null);
        var contentType = _response!.Content.Headers.ContentType?.MediaType;
        Assert.That(contentType, Does.Contain(expectedContentType));
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetResponseContent()
    {
        Assert.That(_response, Is.Not.Null, "Response should not be null");
        return await _response!.Content.ReadAsStringAsync();
    }

    private Dictionary<string, object> TableToDictionary(Table table)
    {
        var dictionary = new Dictionary<string, object>();
        
        if (table.RowCount == 0)
            return dictionary;

        var headers = table.Header.ToList();
        var firstRow = table.Rows[0];

        foreach (var header in headers)
        {
            dictionary[header] = firstRow[header];
        }

        return dictionary;
    }

    #endregion
}
