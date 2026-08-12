using GP_API.Models;

namespace GP_API.Repository.Interfaces
{
    public interface IChildrenRepository : IRepository<Child>
    {
        Task<IEnumerable<Child>> GetParentChildren(string parentID);
    }
}