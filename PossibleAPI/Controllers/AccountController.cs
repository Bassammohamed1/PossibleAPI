using GP_API.DTOs;
using GP_API.Models;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(ITokenService tokenService, IAuthService authService, UserManager<AppUser> userManager)
        {
            _tokenService = tokenService;
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] UserDTO data)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.Register(data);

                if (result.StatusCode == 200)
                    return Ok(new APIResponse { Message = "User registered.", StatusCode = 200 });

                return BadRequest(new APIResponse { Message = result.Message, StatusCode = 400 });
            }

            return BadRequest(ModelState);
        }

        [HttpPut("UpdateUser")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateUser([FromForm] UserDTO data)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new APIResponse { Message = "Invalid token.", StatusCode = 400 });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest(new APIResponse { Message = "User not found.", StatusCode = 400 });

            var result = await _authService.Update(user, data);

            if (result.StatusCode == 200)
                return Ok(new APIResponse { Message = "User updated.", StatusCode = 200 });

            return BadRequest(new APIResponse { Message = result.Message, StatusCode = 400 });
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO user)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.Login(user);

                if (result.StatusCode == 200)
                    return Ok(new APIResponse { Message = "User logged in.", StatusCode = 200 });

                return BadRequest(new APIResponse { Message = result.Message, StatusCode = 400 });
            }
            return BadRequest(ModelState);
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new APIResponse { Message = "Can't find userID.", StatusCode = 400 });

            await _tokenService.InvalidateToken(userId);

            return Ok(new APIResponse { Message = "Logged out successfully.", StatusCode = 200 });
        }

        [HttpGet("GetUserData")]
        [Authorize(Roles = "User,Specialist,Admin")]
        public async Task<IActionResult> GetUserData()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new APIResponse { Message = "Invalid token.", StatusCode = 400 });

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest(new APIResponse { Message = "User not found.", StatusCode = 400 });

            var userRoles = await _userManager.GetRolesAsync(user);

            var data = new UserViewDTO
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Image = user.Image,
                Roles = userRoles.ToList()
            };

            return Ok(data);
        }
    }
}