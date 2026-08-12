using FakeItEasy;
using GP_API.DTOs;
using GP_API.Models;
using GP_API.Repository.Interfaces;
using GP_API.Services;

namespace PossibleAPITests
{
    public class SpecialistsServiceTests
    {
        [Fact]
        public async Task GetChildTests_WhenThereIsNoTests_ReturnEmptyEnumerable()
        {
            //arrange
            var uow = A.Fake<IUnitOfWork>();

            var sut = new SpecialistsService(uow);

            A.CallTo(() => uow.TestChildren.GetChildTestIDs(A<int>.Ignored))
                .Returns(Enumerable.Empty<int>().AsQueryable());

            A.CallTo(() => uow.Tests.Get(A<int>.Ignored))
                .Returns(Task.FromResult<Test>(null));

            A.CallTo(() => uow.Questions.GetTestQuestions(A<int>.Ignored))
                .Returns(Enumerable.Empty<Question>().AsQueryable());

            //act 
            var result = await sut.GetChildTests(1);

            //assert
            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public async Task GetChildTests_WhenThereIsTests_ReturnTests()
        {
            //arrange
            var uow = A.Fake<IUnitOfWork>();

            var questions = new List<Question>()
            {
                new Question()
                {
                    QuestionText = "QuestionText", QuestionAnswer = "QuestionAnswer" , QuestionType = "QuestionType" , TestId = 1
                },
                new Question()
                {
                    QuestionText = "QuestionText", QuestionAnswer = "QuestionAnswer" , QuestionType = "QuestionType" ,TestId = 1
                },
                new Question()
                {
                    QuestionText = "QuestionText", QuestionAnswer = "QuestionAnswer" , QuestionType = "QuestionType" ,TestId = 2
                }
            };

            var ids = new List<int> { 1, 2, 3, 4 };

            A.CallTo(() => uow.TestChildren.GetChildTestIDs(A<int>.Ignored))
                .Returns(ids.AsQueryable());

            A.CallTo(() => uow.Tests.Get(A<int>.Ignored))
                .Returns(Task.FromResult(new Test()));

            A.CallTo(() => uow.Questions.GetTestQuestions(A<int>.Ignored))
                .Returns(questions.AsQueryable());

            var sut = new SpecialistsService(uow);

            //act 
            var result = await sut.GetChildTests(1);

            //assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Equal(3, result.First().Questions.Count());
        }

        [Fact]
        public async Task CreateTest_WhenThereIsErrorWhileAddingTest_ReturnStatusCode400()
        {
            //arrange
            var uow = A.Fake<IUnitOfWork>();

            var sut = new SpecialistsService(uow);

            A.CallTo(() => uow.Tests.Add(A<Test>.Ignored))
                .Returns(Task.FromResult<Test>(null));

            //act
            var result = await sut.CreateTest(new CreateTestDTO());

            //assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while adding test.", result.Message);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenThereIsErrorWhileAddingTestChildren_ReturnStatusCode400()
        {
            //arrange
            var uow = A.Fake<IUnitOfWork>();

            var sut = new SpecialistsService(uow);

            A.CallTo(() => uow.Tests.Add(A<Test>.Ignored))
                .Returns(Task.FromResult<Test>(new Test()));

            A.CallTo(() => uow.TestChildren.Add(A<TestChildren>.Ignored))
                .Returns(Task.FromResult<TestChildren>(null));

            //act
            var result = await sut.CreateTest(new CreateTestDTO() { ChildrenId = new List<int> { 1, 2, 3 } });

            //assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while adding testchildren.", result.Message);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenThereIsErrorWhileAddingQuestions_ReturnStatusCode400()
        {
            //arrange
            var uow = A.Fake<IUnitOfWork>();

            var sut = new SpecialistsService(uow);

            A.CallTo(() => uow.Tests.Add(A<Test>.Ignored))
                .Returns(Task.FromResult<Test>(new Test()));

            A.CallTo(() => uow.TestChildren.Add(A<TestChildren>.Ignored))
             .Returns(Task.FromResult<TestChildren>(new TestChildren()));

            A.CallTo(() => uow.Questions.Add(A<Question>.Ignored))
             .Returns(Task.FromResult<Question>(null));

            //act
            var result = await sut.CreateTest(new CreateTestDTO() { Questions = new List<QuestionDTO> { new QuestionDTO(), new QuestionDTO() }, ChildrenId = new List<int> { 1, 2, 3 } });

            //assert
            Assert.NotNull(result);
            Assert.Equal("An error occurred while adding questions.", result.Message);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenDataIsValid_AddTestSuccessfully()
        {
            //arrange
            var uow = A.Fake<IUnitOfWork>();

            var sut = new SpecialistsService(uow);

            A.CallTo(() => uow.Tests.Add(A<Test>.Ignored))
                .Returns(Task.FromResult<Test>(new Test()));

            A.CallTo(() => uow.TestChildren.Add(A<TestChildren>.Ignored))
             .Returns(Task.FromResult<TestChildren>(new TestChildren()));

            A.CallTo(() => uow.Questions.Add(A<Question>.Ignored))
             .Returns(Task.FromResult<Question>(new Question()));

            //act
            var result = await sut.CreateTest(new CreateTestDTO() { Questions = new List<QuestionDTO> { new QuestionDTO(), new QuestionDTO() }, ChildrenId = new List<int> { 1, 2, 3 } });

            //assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
    }
}
