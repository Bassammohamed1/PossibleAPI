using GP_API.Models;

namespace GP_API.Repository.Interfaces
{
    public interface ITestChildrenRepoository : IRepository<TestChildren>
    {
        IQueryable<int> GetChildTestIDs(int childID);
    }
}
