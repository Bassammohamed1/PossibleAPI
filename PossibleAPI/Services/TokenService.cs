using GP_API.Repository.Interfaces;
using GP_API.Services.Interfaces;

namespace GP_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task InvalidateToken(string userID)
        {
            var token = await _unitOfWork.Tokens.GetUserToken(userID);

            if (token != null)
            {
                _unitOfWork.Tokens.Delete(token);
                await _unitOfWork.Commit();
            }
        }
    }
}
