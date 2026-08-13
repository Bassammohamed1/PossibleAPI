using GP_API.Models;

namespace GP_API.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        IChildrenRepository Children { get; }
        IRepository<Test> Tests { get; }
        IQuestionsRepository Questions { get; }
        ITestChildrenRepoository TestChildren { get; }
        ITokensRepository Tokens { get; }
        Task Commit();
    }
}
