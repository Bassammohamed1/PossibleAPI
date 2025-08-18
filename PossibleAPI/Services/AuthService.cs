using Azure.Core;
using GP_API.Data;
using GP_API.Models;
using GP_API.Models.DTOs;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GP_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(AppDbContext context, IWebHostEnvironment environment, UserManager<AppUser> userManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthModel> Register(UserDTO data)
        {
            var webRootPath = _environment.WebRootPath;

            if (data.ClientFile == null)
            {
                return new AuthModel { Message = "Client file is missing.", StatusCode = 404 };
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(data.ClientFile.FileName);
            var filePath = Path.Combine(webRootPath, "files/uploads/images", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await data.ClientFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return new AuthModel { StatusCode = 500, Message = "Error saving file: " + ex.Message };
            }

            var request = _httpContextAccessor.HttpContext.Request;

            var user = new AppUser()
            {
                UserName = data.UserName,
                Email = data.Email,
                Image = $"{request.Scheme}://{request.Host}/files/uploads/images/{fileName}"
            };

            var result = await _userManager.CreateAsync(user, data.Password);

            if (result.Succeeded)
            {
                switch (data.RoleNo)
                {
                    case 1:
                        await _userManager.AddToRoleAsync(user, "User");
                        break;
                    case 2:
                        await _userManager.AddToRoleAsync(user, "Specialist");
                        break;
                    default:
                        return new AuthModel { StatusCode = 404, Message = "RoleNo must be 1 or 2." };
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                var model = new AuthModel
                {
                    StatusCode = 200,
                    Token = await this.GenerateToken(user),
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Image = user.Image,
                    Roles = userRoles
                };

                var userToken = new UserToken
                {
                    UserId = user.Id,
                    Token = model.Token
                };

                _context.Tokens.Add(userToken);
                await _context.SaveChangesAsync();

                return model;
            }
            else
            {
                return new AuthModel { StatusCode = 404, Message = string.Join('-', result.Errors) };
            }
        }

        public async Task<AuthModel> Login(LoginDTO data)
        {
            var user = await _userManager.FindByEmailAsync(data.Email);

            if (user is not null)
            {
                if (await _userManager.CheckPasswordAsync(user, data.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var model = new AuthModel
                    {
                        Token = await this.GenerateToken(user),
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Image = user.Image,
                        Roles = userRoles,
                        StatusCode = 200
                    };

                    var userToken = new UserToken
                    {
                        UserId = user.Id,
                        Token = model.Token
                    };

                    _context.Tokens.Add(userToken);
                    await _context.SaveChangesAsync();

                    return model;
                }

                return new AuthModel { Message = "Invalid email or password", StatusCode = 401 };
            }

            return new AuthModel { Message = "Invalid email or password", StatusCode = 401 };
        }

        public async Task<AuthModel> Update(AppUser user, UserDTO data)
        {
            var webRootPath = _environment.WebRootPath;

            if (data.ClientFile == null)
            {
                return new AuthModel { Message = "Client file is missing", StatusCode = 404 };
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(data.ClientFile.FileName);
            var filePath = Path.Combine(webRootPath, "files/uploads/images", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await data.ClientFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return new AuthModel { StatusCode = 500, Message = "Error saving file: " + ex.Message };
            }

            var request = _httpContextAccessor.HttpContext.Request;

            var userRole = await _userManager.GetRolesAsync(user);
            string role = userRole.First();

            user.UserName = data.UserName;
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, data.Password);
            user.Email = data.Email;
            user.Image = $"{request.Scheme}://{request.Host}/files/uploads/images/{fileName}";

            if (data.RoleNo == 1 && role != "User")
            {
                await _userManager.RemoveFromRoleAsync(user, "Specialist");
                await _userManager.AddToRoleAsync(user, "User");
            }
            else if (data.RoleNo == 2 && role != "Specialist")
            {
                await _userManager.RemoveFromRoleAsync(user, "User");
                await _userManager.AddToRoleAsync(user, "Specialist");
            }

            var result = await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);

            if (!result.Succeeded)
                return new AuthModel { StatusCode = 500, Message = string.Join('-', result.Errors) };

            return new AuthModel
            {
                StatusCode = 200,
                UserId = user.Id,
                Email = data.Email,
                Image = user.Image,
                UserName = data.UserName,
                Roles = userRole
            };
        }

        private async Task<string> GenerateToken(AppUser user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var sc = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: sc
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
