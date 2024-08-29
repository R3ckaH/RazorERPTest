using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RazorERPTest.Interfaces;
using RazorERPTest.Models;


namespace RazorERPTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var token = await _authService.AuthenticateAsync(loginRequest);
            if (token == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { Token = token });
        }


    }
}
