using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Services;

namespace PostService.Controllers;

[Route("api/v1/post")]
[ApiController]
public class PostControllerV1 : ControllerBase
{
    public PostControllerV1()
    {
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running (v1)");
    }    
}

[Route("api/v2/post")]
[ApiController]
public class PostControllerV2 : ControllerBase
{
    private readonly IDValidationService _idValidationService;
    private readonly PostDbContext _context;

    public PostControllerV2(IDValidationService idValidationService, PostDbContext context)
    {
        _idValidationService = idValidationService;
        _context = context; 
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running (v2)");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        return await _context.Posts.ToListAsync();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetPostsByUserId(Guid userId)
    {
        var posts = await _context.Posts
            .Where(p => p.UserId == userId.ToString())
            .ToListAsync();

        if (!posts.Any())
        {
            return NotFound(new { Message = $"No posts found for user with ID {userId}." });
        }

        return Ok(posts);
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
