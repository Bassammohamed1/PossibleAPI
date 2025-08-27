using GP_API.Models;
using GP_API.Models.DTOs;

namespace GP_API.Services.Interfaces
{
    public interface IChildrenService
    {
        Task<IEnumerable<Child>> GetAllChildren();
        Task<Child> GetChildById(int id);
        Task<List<Child>> GetChildrenByParentId(string id);
        Task<DbOperationModel> AddChild(ChildDTO data);
        Task<DbOperationModel> UpdateChild(Child child, ChildDTO data);
        Task DeleteChild(Child child);
        Task SaveChanges();
    }
}
