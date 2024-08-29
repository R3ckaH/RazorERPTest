using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RazorERPTest.Interfaces;
using RazorERPTest.Services;
using RazorERPTest.Models;
using Microsoft.AspNetCore.Identity.Data;


namespace RazorERPTest.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public UsersController(IUserRepository userRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        // Admin-only endpoint
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.PasswordHash = hashedPassword;

            var userId = await _userRepository.CreateUserAsync(user);
            return Ok(new { Id = userId });
        }

        // Admin-only endpoint
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            user.Id = id;
            var updated = await _userRepository.UpdateUserAsync(user);
            if (updated)
                return NoContent();
            return NotFound();
        }

        // Admin-only endpoint
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userRepository.DeleteUserAsync(id);
            if (deleted)
                return NoContent();
            return NotFound();
        }


        [HttpGet("ListUsers")]
        public async Task<IActionResult> ListUsers()
        {
            var users = User.IsInRole("Admin")
                ? await _userRepository.GetUsersByCompanyAsync(int.Parse(User.FindFirst("CompanyId").Value))
                : await _userRepository.GetNonAdminUsersByCompanyAsync(int.Parse(User.FindFirst("CompanyId").Value));

            return Ok(users);
        }


    }
}
