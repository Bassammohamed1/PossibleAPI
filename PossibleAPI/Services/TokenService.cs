using GP_API.Data;
using GP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GP_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;

        public TokenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task InvalidateToken(string userId)
        {
            var token = await _context.Tokens.FirstOrDefaultAsync(t => t.UserId == userId);

            if (token != null)
            {
                _context.Tokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }
    }
}
