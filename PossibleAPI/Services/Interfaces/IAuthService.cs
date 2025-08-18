using GP_API.Models;
using GP_API.Models.DTOs;

namespace GP_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthModel> Register(UserDTO data);
        Task<AuthModel> Login(LoginDTO data);
        Task<AuthModel> Update(AppUser user, UserDTO data);
    }
}
