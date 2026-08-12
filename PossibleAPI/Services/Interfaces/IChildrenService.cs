using GP_API.DTOs;
using GP_API.Helpers;
using GP_API.Models;

namespace GP_API.Services.Interfaces
{
    public interface IChildrenService
    {
        Task<IEnumerable<Child>> GetAllChildren();
        Task<Child> GetChildById(int id);
        Task<IEnumerable<Child>> GetChildrenByParentId(string id);
        Task<Result> AddChild(ChildDTO data);
        Task<Result> UpdateChild(Child child, ChildDTO data);
        Task<Result> DeleteChild(Child child);
        Task SaveChanges();
    }
}
