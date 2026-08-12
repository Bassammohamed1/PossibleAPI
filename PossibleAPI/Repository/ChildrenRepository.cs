using GP_API.Data;
using GP_API.Models;
using GP_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GP_API.Repository
{
    public class ChildrenRepository : Repository<Child>, IChildrenRepository
    {
        private readonly AppDbContext _context;

        public ChildrenRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Child>> GetParentChildren(string parentID)
        {
            return await _context.Children.Where(x => x.ParentId == parentID).ToListAsync();
        }
    }
}
