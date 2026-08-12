using GP_API.Data;
using GP_API.Models;
using GP_API.Repository.Interfaces;

namespace GP_API.Repository
{
    public class QuestionsRepository : Repository<Question>, IQuestionsRepository
    {
        private readonly AppDbContext _context;

        public QuestionsRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }

        public IQueryable<Question> GetTestQuestions(int testID)
        {
            return _context.Questions.Where(q => q.TestId == testID);
        }
    }
}
