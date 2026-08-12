using GP_API.DTOs;
using GP_API.Models;

namespace GP_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthModel> Register(UserDTO data);
        Task<AuthModel> Login(LoginDTO data);
        Task<AuthModel> Update(AppUser user, UserDTO data);
    }
}
