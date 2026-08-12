
using FakeItEasy;
using GP_API.Controllers;
using GP_API.DTOs;
using GP_API.Helpers;
using GP_API.Models;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PossibleAPITests
{
    public class SpecialistsControllerTests
    {
        [Fact]
        public async Task GetChildTests_WhenThereIsNoTests_ReturnOkWithEmptyEnumerable()
        {
            //arrange
            var service = A.Fake<ISpecialistsService>();

            A.CallTo(() => service.GetChildTests(A<int>.Ignored))
                .Returns(Task.FromResult<List<TestDTO>>(Enumerable.Empty<TestDTO>().ToList()));

            var sut = new SpecialistsController(service);

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
            var service = A.Fake<ISpecialistsService>();

            A.CallTo(() => service.GetChildTests(A<int>.Ignored))
                .Returns(Task.FromResult<List<TestDTO>>(new List<TestDTO>() { new TestDTO(), new TestDTO() }));

            var sut = new SpecialistsController(service);

            //act 
            var result = await sut.GetChildTests(1);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as List<TestDTO>;
            Assert.NotNull(data);
            Assert.Equal(2, data.Count);

        }

        [Fact]
        public async Task CreateTest_WhenTestNameIsNull_ReturnBadRequest()
        {
            //arrange
            var service = A.Fake<ISpecialistsService>();

            var sut = new SpecialistsController(service);

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
            var service = A.Fake<ISpecialistsService>();

            var sut = new SpecialistsController(service);

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
            var service = A.Fake<ISpecialistsService>();

            var sut = new SpecialistsController(service);

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
            var service = A.Fake<ISpecialistsService>();

            var sut = new SpecialistsController(service);

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
        public async Task CreateTest_WhenThereIsAnErrorWileCreatingTest_ReturnBadRequest()
        {
            //arrange
            var service = A.Fake<ISpecialistsService>();

            var sut = new SpecialistsController(service);

            A.CallTo(() => service.CreateTest(A<CreateTestDTO>.Ignored))
                .Returns(Task.FromResult(new Result { StatusCode = 400, Message = "An error occuured while adding." }));

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
                ChildrenId = new List<int> { 1, 2, 3 }
            };

            //act
            var result = await sut.CreateTest(test);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("An error occuured while adding.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task CreateTest_WhenThereIsAnErrorWileCreatingTest_ReturnOkAndCreateTest()
        {
            //arrange
            var service = A.Fake<ISpecialistsService>();

            var sut = new SpecialistsController(service);

            A.CallTo(() => service.CreateTest(A<CreateTestDTO>.Ignored))
                .Returns(Task.FromResult(new Result { StatusCode = 200 }));

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
                ChildrenId = new List<int> { 1, 2, 3 }
            };

            //act
            var result = await sut.CreateTest(test);
            var okRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(okRequestResult);
            Assert.Equal(200, okRequestResult.StatusCode);
            Assert.NotNull(okRequestResult.Value);

            var data = okRequestResult.Value as APIResponse;
            Assert.Equal(200, data.StatusCode);
        }
    }
}