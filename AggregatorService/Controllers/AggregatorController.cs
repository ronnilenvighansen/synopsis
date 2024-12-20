using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/v2/aggregator")]
public class AggregatorController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AggregatorController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAggregatedUserData(Guid userId)
    {
        var userResponse = await _httpClientFactory.CreateClient().GetAsync($"http://userservice/api/v2/user/{userId}");
        if (!userResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)userResponse.StatusCode, "Failed to retrieve user data.");
        }

        var userContent = await userResponse.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<User>(userContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var postResponse = await _httpClientFactory.CreateClient().GetAsync($"http://postservice/api/v2/post/user/{userId}");
        if (!postResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)postResponse.StatusCode, "Failed to retrieve post data.");
        }

        var postContent = await postResponse.Content.ReadAsStringAsync();
        var postData = JsonSerializer.Deserialize<List<Post>>(postContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var aggregatedData = new
        {
            User = userData,
            Posts = postData
        };

        return Ok(aggregatedData);
    }
}
