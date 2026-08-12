using GP_API.DTOs;
using GP_API.Helpers;

namespace GP_API.Services.Interfaces
{
    public interface ISpecialistsService
    {
        Task<List<TestDTO>> GetChildTests(int childID);
        Task<Result> CreateTest(CreateTestDTO data);
    }
}
