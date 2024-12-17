using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Services;

namespace PostService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IDValidationService _idValidationService;
    private readonly PostDbContext _context;
    private readonly HttpClient _httpClient;

    public PostController(IDValidationService idValidationService, PostDbContext context, IHttpClientFactory httpClientFactory)
    {
        _idValidationService = idValidationService;
        _context = context; 
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        return await _context.Posts.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost([FromBody] Post post)
    {   
        try
        {
            var isValidID = await _idValidationService.ValidateIDAsync(post.UserId);
            if (!isValidID)
            {
                return BadRequest("Invalid ID.");
            }

            post.CreatedAt = DateTime.UtcNow;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPosts), new { id = post.Id }, post);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user: {ex.Message}");
            return StatusCode(500, "User service communication error.");
        }
    }

   [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] Post updatedPost)
    {
        if (id != updatedPost.Id)
            return BadRequest("Post ID mismatch.");

        var existingPost = await _context.Posts.FindAsync(id);
        if (existingPost == null)
            return NotFound("Post not found.");

        existingPost.Content = updatedPost.Content;
        existingPost.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
