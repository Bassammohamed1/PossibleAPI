using GP_API.Data;
using GP_API.Models;
using GP_API.Repository.Interfaces;

namespace GP_API.Repository
{
    public class TestChildrenRepository : Repository<TestChildren>, ITestChildrenRepoository
    {
        private readonly AppDbContext _context;

        public TestChildrenRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<int> GetChildTestIDs(int childID)
        {
            return _context.TestChildren.Where(t => t.ChildId == childID)
               .Select(t => t.TestId).Distinct();
        }
    }
}
