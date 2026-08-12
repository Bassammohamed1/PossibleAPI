using GP_API.DTOs;
using GP_API.Helpers;
using GP_API.Models;
using GP_API.Repository.Interfaces;
using GP_API.Services.Interfaces;

namespace GP_API.Services
{
    public class SpecialistsService : ISpecialistsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecialistsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> CreateTest(CreateTestDTO data)
        {
            var test = new Test()
            {
                Name = data.TestName,
                Category = data.TestCategory,
                QuestionNo = data.QuestionsNo
            };

            var addedTest = await _unitOfWork.Tests.Add(test);
            await _unitOfWork.Commit();

            if (addedTest is null)
                return new Result { StatusCode = 400, Message = "An error occurred while adding test." };

            var testChildrenList = new List<TestChildren>();
            var questionsList = new List<Question>();

            foreach (var childId in data.ChildrenId)
            {
                var data1 = new TestChildren()
                {
                    ChildId = childId,
                    TestId = test.Id
                };
                var result = await _unitOfWork.TestChildren.Add(data1);
                testChildrenList.Add(result);
            }
            await _unitOfWork.Commit();

            if (testChildrenList.Any(tc => tc is null))
                return new Result { StatusCode = 400, Message = "An error occurred while adding testchildren." };

            foreach (var question in data.Questions)
            {
                var data2 = new Question()
                {
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    QuestionAnswer = question.QuestionAnswer,
                    TestId = test.Id,
                };
                var result = await _unitOfWork.Questions.Add(data2);

                questionsList.Add(result);
            }
            await _unitOfWork.Commit();

            if (questionsList.Any(q => q is null))
                return new Result { StatusCode = 400, Message = "An error occurred while adding questions." };

            return new Result { StatusCode = 200 };
        }

        public async Task<List<TestDTO>> GetChildTests(int childID)
        {
            var testIDs = _unitOfWork.TestChildren.GetChildTestIDs(childID);

            var testData = new List<TestDTO>();

            foreach (var testID in testIDs)
            {
                var test = await _unitOfWork.Tests.Get(testID);

                if (test != null)
                {
                    var questions = _unitOfWork.Questions.GetTestQuestions(testID);

                    var testDTO = new TestDTO()
                    {
                        TestName = test.Name,
                        TestCategory = test.Category,
                        QuestionsNo = test.QuestionNo,
                        Questions = questions.Select(q => new QuestionDTO
                        {
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            QuestionAnswer = q.QuestionAnswer
                        }).ToList()
                    };

                    testData.Add(testDTO);
                }
            }

            return testData;
        }
    }
}
