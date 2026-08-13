using GP_API.Data;
using GP_API.Models;
using GP_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GP_API.Repository
{
    public class TokensRepository : Repository<UserToken>, ITokensRepository
    {
        private readonly AppDbContext _context;

        public TokensRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<UserToken> GetUserToken(string userID)
        {
            return await _context.Tokens.FirstOrDefaultAsync(x => x.UserId == userID);
        }
    }
}
