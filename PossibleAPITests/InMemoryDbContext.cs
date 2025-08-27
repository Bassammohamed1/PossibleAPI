using GP_API.Data;
using Microsoft.EntityFrameworkCore;

namespace PossibleAPITests
{
    public class InMemoryDbContext : AppDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }
        
        public override void Dispose()
        {
            Database.EnsureDeleted();
            base.Dispose();
        }
    }
}
