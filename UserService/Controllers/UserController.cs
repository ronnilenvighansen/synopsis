using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UserService.Controllers;

[ApiController]
[Route("api/v1/user")]
public class UserControllerV1 : ControllerBase
{
    public UserControllerV1()
    {
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running (v1)");
    }
}

[ApiController]
[Route("api/v2/user")]
public class UserControllerV2 : ControllerBase
{
    private readonly UserDbContext _context;

    public UserControllerV2(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Service is running (v2)");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {userId} not found." });
        }

        return Ok(user);
    }

    [HttpGet("validate-id/{userId}")]
    public async Task<IActionResult> ValidateID(string userId)
    {
        Console.WriteLine($"Validating ID: {userId}");

        if (!Guid.TryParse(userId, out var userIdGuid))
        {
            Console.WriteLine($"Invalid ID format: {userId}");
            return BadRequest("Invalid ID format.");
        }

        var user = await _context.Users.FindAsync(userIdGuid);

        if (user == null)
        {
            Console.WriteLine($"ID {userId} not found.");
            return NotFound("ID not found.");
        }

        Console.WriteLine($"ID {userId} is valid.");
        return Ok("ID is valid.");
    }


    [HttpPost]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User updateRequest)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found.");

        user.FullName = updateRequest.FullName ?? user.FullName;
        user.Email = updateRequest.Email ?? user.Email;

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
