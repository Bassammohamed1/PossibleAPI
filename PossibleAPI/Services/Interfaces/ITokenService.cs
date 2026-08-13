
namespace GP_API.Services.Interfaces
{
    public interface ITokenService
    {
        Task InvalidateToken(string userID);
    }
}