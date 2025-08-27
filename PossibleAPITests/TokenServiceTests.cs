using FakeItEasy;
using GP_API.Models;
using GP_API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PossibleAPITests
{
    public class TokenServiceTests
    {
        [Fact]
        public async Task InvalidateToken_WhereTokenExists_InvalidateTokenSuccessfully()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new TokenService(context);

            var token = new UserToken()
            {
                Token = "Test token 123.",
                UserId = "User ID #123"
            };

            await context.Tokens.AddAsync(token);
            await context.SaveChangesAsync();

            //act
            sut.InvalidateToken("User ID #123");

            //assert
            Assert.Empty(context.Tokens.Where(t => t.UserId == token.UserId));
        }
    }
}
