using GP_API.Data;
using GP_API.Models;
using GP_API.Repository.Interfaces;

namespace GP_API.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Children = new ChildrenRepository(context);
            Tests = new Repository<Test>(context);
            Questions = new QuestionsRepository(context);
            TestChildren = new TestChildrenRepository(context);
            Tokens = new TokensRepository(context);
        }

        public IChildrenRepository Children { get; private set; }
        public IRepository<Test> Tests { get; private set; }
        public IQuestionsRepository Questions { get; private set; }
        public ITestChildrenRepoository TestChildren { get; private set; }
        public ITokensRepository Tokens { get; private set; }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }

        public async Task Dispose()
        {
            await _context.DisposeAsync();
        }
    }
}
