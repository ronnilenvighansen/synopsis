using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/v2/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;

    public AuthenticationController(JwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("all")]
    public IActionResult LoginAll()
    {
        var token = _jwtTokenService.CreateAllToken();
        return Ok(token);
    }
}
