
using GP_API.Controllers;
using GP_API.Models;
using GP_API.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace PossibleAPITests
{
    public class SpecialistsControllerTests
    {
        [Fact]
        public async Task GetChildTests_WhenThereIsNoTests_ReturnEmptyEnumerable()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new SpecialistsController(context);

            //act 
            var result = await sut.GetChildTests(1);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as List<TestDTO>;
            Assert.Empty(data);
        }

        [Fact]
        public async Task GetChildTests_WhenThereIsTests_ReturnTests()
        {
            //arrange
            var context = new InMemoryDbContext();

            var tests = new List<Test>()
            {
                new Test(){Name = "Test #1" ,Category = "Category #1" , QuestionNo = 3 },
                new Test(){Name = "Test #2" ,Category = "Category #2" , QuestionNo = 3 },
                new Test(){Name = "Test #3" ,Category = "Category #3" , QuestionNo = 3 }
            };

            await context.Tests.AddRangeAsync(tests);

            var questions = new List<Question>()
            {
                new Question()
                {
                    QuestionText = "QuestionText", QuestionAnswer = "QuestionAnswer" , QuestionType = "QuestionType" , TestId = tests.First().Id
                },
                new Question()
                {
                    QuestionText = "QuestionText", QuestionAnswer = "QuestionAnswer" , QuestionType = "QuestionType" ,TestId = tests.First().Id
                },
                new Question()
                {
                    QuestionText = "QuestionText", QuestionAnswer = "QuestionAnswer" , QuestionType = "QuestionType" ,TestId = tests.First().Id
                }
            };

            await context.Questions.AddRangeAsync(questions);

            var testChildren = new List<TestChildren>()
            {
                new TestChildren(){ChildId = 1, TestId = tests[0].Id},
                new TestChildren(){ChildId = 1, TestId = tests[1].Id},
                new TestChildren(){ChildId = 1, TestId = tests[2].Id},
                new TestChildren(){ChildId = 2, TestId = tests[0].Id},
                new TestChildren(){ChildId = 2, TestId = tests[1].Id},
                new TestChildren(){ChildId = 2, TestId = tests[2].Id}
            };

            await context.TestChildren.AddRangeAsync(testChildren);

            await context.SaveChangesAsync();

            var sut = new SpecialistsController(context);

            //act 
            var result = await sut.GetChildTests(1);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as List<TestDTO>;
            Assert.NotNull(data);
            Assert.Equal(3, data.Count);

        }

        [Fact]
        public async Task CreateTest_WhenTestNameIsNull_ReturnBadRequest()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new SpecialistsController(context);

            var test = new CreateTestDTO()
            {
                QuestionsNo = 2,
                TestCategory = "Category #1",
                Questions = new List<QuestionDTO>()
                {
                    new QuestionDTO()
                    {

                        QuestionType = "QuestionType #1",
                        QuestionAnswer = "QuestionAnswer #1",
                        QuestionText = "QuestionText #1"
                    },
                    new QuestionDTO()
                    {
                        QuestionType = "QuestionType #2",
                        QuestionAnswer = "QuestionAnswer #2",
                        QuestionText = "QuestionText #2"
                    },
                },
                ChildrenId = new List<int> { 1, 2 }
            };

            //act
            var result = await sut.CreateTest(test);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("Test name or test category is null.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenTestCategoryIsNull_ReturnBadRequest()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new SpecialistsController(context);

            var test = new CreateTestDTO()
            {
                TestName = "Test",
                QuestionsNo = 2,
                Questions = new List<QuestionDTO>()
                {
                    new QuestionDTO()
                    {

                        QuestionType = "QuestionType #1",
                        QuestionAnswer = "QuestionAnswer #1",
                        QuestionText = "QuestionText #1"
                    },
                    new QuestionDTO()
                    {
                        QuestionType = "QuestionType #2",
                        QuestionAnswer = "QuestionAnswer #2",
                        QuestionText = "QuestionText #2"
                    },
                },
                ChildrenId = new List<int> { 1, 2 }
            };

            //act
            var result = await sut.CreateTest(test);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("Test name or test category is null.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenQuestionsIsEmpty_ReturnBadRequest()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new SpecialistsController(context);

            var test = new CreateTestDTO()
            {
                TestName = "Test's name",
                QuestionsNo = 2,
                TestCategory = "Category #1",
                ChildrenId = new List<int> { 1, 2 },
                Questions = Enumerable.Empty<QuestionDTO>().ToList()
            };

            //act
            var result = await sut.CreateTest(test);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("Questions or childrenIDs can't be empty.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenChildrenIDsIsEmpty_ReturnBadRequest()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new SpecialistsController(context);

            var test = new CreateTestDTO()
            {
                TestName = "Test's name",
                QuestionsNo = 2,
                TestCategory = "Category #1",
                Questions = new List<QuestionDTO>()
                {
                     new QuestionDTO()
                     {

                         QuestionType = "QuestionType #1",
                         QuestionAnswer = "QuestionAnswer #1",
                         QuestionText = "QuestionText #1"
                     },
                     new QuestionDTO()
                     {
                         QuestionType = "QuestionType #2",
                         QuestionAnswer = "QuestionAnswer #2",
                         QuestionText = "QuestionText #2"
                     },
                },
                ChildrenId = Enumerable.Empty<int>().ToList()
            };

            //act
            var result = await sut.CreateTest(test);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("Questions or childrenIDs can't be empty.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenDataIsValid_AddTestSuccessfully()
        {
            //arrange
            var context = new InMemoryDbContext();

            var sut = new SpecialistsController(context);

            var test = new CreateTestDTO()
            {
                TestName = "Test's name",
                QuestionsNo = 2,
                TestCategory = "Category #1",
                Questions = new List<QuestionDTO>()
                {
                    new QuestionDTO()
                    {

                        QuestionType = "QuestionType #1",
                        QuestionAnswer = "QuestionAnswer #1",
                        QuestionText = "QuestionText #1"
                    },
                    new QuestionDTO()
                    {
                        QuestionType = "QuestionType #2",
                        QuestionAnswer = "QuestionAnswer #2",
                        QuestionText = "QuestionText #2"
                    },
                },
                ChildrenId = new List<int> { 1, 2 }
            };

            //act
            var result = await sut.CreateTest(test);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as APIResponse;
            Assert.Equal("Test has been created successfully.", data.Message);
            Assert.Equal(200, data.StatusCode);
        }
    }
}