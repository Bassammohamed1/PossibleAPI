using GP_API.Models;

namespace GP_API.Repository.Interfaces
{
    public interface IQuestionsRepository : IRepository<Question>
    {
        IQueryable<Question> GetTestQuestions(int testID);
    }
}
