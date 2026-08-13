using GP_API.Models;

namespace GP_API.Repository.Interfaces
{
    public interface ITokensRepository : IRepository<UserToken>
    {
        Task<UserToken> GetUserToken(string userID);
    }
}
