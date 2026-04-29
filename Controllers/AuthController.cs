using FileService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("token")]
        public IActionResult GenerateToken()
        {
            var token = _jwtService.GenerateToken();
            return Ok(new { token });
        }

        // Endpoint for generating a fixed token for development purposes
        [HttpGet("fixed-token")]
        public IActionResult GetFixedToken()
        {
            // In a real application, never expose a fixed token like this in production!
            // This is only for development purposes
            var fixedToken = _jwtService.GenerateToken(); // This generates a new token each time with default claims
            return Ok(new { token = fixedToken, message = "This is a generated token. For development purposes only!" });
        }
    }
}