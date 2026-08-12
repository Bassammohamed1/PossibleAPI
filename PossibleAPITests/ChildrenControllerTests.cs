
using FakeItEasy;
using GP_API.Controllers;
using GP_API.DTOs;
using GP_API.Helpers;
using GP_API.Models;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PossibleAPITests
{
    public class ChildrenControllerTests
    {
        [Fact]
        public async Task GetAllChildren_WhenThereIsNoChildren_ReturnEmptyEnumerable()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            //act
            var result = await sut.GetAllChildren();
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Empty(okResult.Value as IEnumerable<Child>);
        }

        [Fact]
        public async Task GetAllChildren_WhenThereIsChildren_ReturnChildren()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            A.CallTo(() => childrenService.GetAllChildren())
                .Returns(new List<Child>()
                {
                    new Child(),
                    new Child(),
                    new Child(),
                    new Child()
                });

            var sut = new ChildrenController(childrenService, userManager);

            //act
            var result = await sut.GetAllChildren();
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var data = okResult.Value as IEnumerable<Child>;
            Assert.NotEmpty(data);
            Assert.Equal(4, data.Count());


        }

        [Fact]
        public async Task GetChildById_WhenTheIDIsZero_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            //act
            var result = await sut.GetChildById(0);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("Invalid id.", data.Message);
        }

        [Fact]
        public async Task GetChildById_WhenChildNull_ReturnNotFound()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(Task.FromResult<Child?>(null));

            var sut = new ChildrenController(childrenService, userManager);

            //act
            var result = await sut.GetChildById(1);
            var notFoundResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);

            var data = notFoundResult.Value as APIResponse;
            Assert.Equal(404, data.StatusCode);
            Assert.Equal("Child not found.", data.Message);
        }

        [Fact]
        public async Task GetChildById_WhenChildIsFound_ReturnChild()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(new Child());

            var sut = new ChildrenController(childrenService, userManager);

            //act
            var result = await sut.GetChildById(1);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetUserChildren_WhenUserIDIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(Task.FromResult<AppUser?>(null));

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            //act
            var result = await sut.GetUserChildren();
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("Invalid token.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task GetUserChildren_WhenUserIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(Task.FromResult<AppUser?>(null));

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "UserID")
                    }))
                }
            };

            //act
            var result = await sut.GetUserChildren();
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("User not found.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task GetUserChildren_WhenChildrenIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(new AppUser());

            A.CallTo(() => childrenService.GetChildrenByParentId(A<string>.Ignored))
                .Returns(Task.FromResult<IEnumerable<Child>?>(null));

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "UserID")
                    }))
                }
            };

            //act
            var result = await sut.GetUserChildren();
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("Invalid parent ID.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task GetUserChildren_WhenChildrenIsNotNull_ReturnChildren()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(new AppUser());

            A.CallTo(() => childrenService.GetChildrenByParentId(A<string>.Ignored))
                .Returns(new List<Child>
                {
                    new Child(),
                    new Child(),
                    new Child(),
                    new Child()
                });

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "UserID")
                    }))
                }
            };

            //act
            var result = await sut.GetUserChildren();
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as List<ChildViewDTO>;
            Assert.Equal(4, data.Count());
        }

        [Fact]
        public async Task AddChild_WhenModelStateIsNotValid_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);
            sut.ModelState.AddModelError("Custom", "Model state is invalid.");

            //act
            var result = await sut.AddChild(null);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task AddChild_WhenThereIsAnErrorInAdding_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.AddChild(A<ChildDTO>.Ignored))
                .Returns(new Result { StatusCode = 400, Message = "There is an error." });

            //act
            var result = await sut.AddChild(null);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(400, okResult.StatusCode);

            var data = okResult.Value as APIResponse;
            Assert.NotNull(data);
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("There is an error.", data.Message);
        }

        [Fact]
        public async Task AddChild_WhenThereIsNotAnyErrors_AddChildSuccessfully()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.AddChild(A<ChildDTO>.Ignored))
                .Returns(new Result { StatusCode = 200, Entity = new Child() });

            //act
            var result = await sut.AddChild(new ChildDTO());
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateChild_WhenModelStateIsNotValid_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);
            sut.ModelState.AddModelError("Custom", "Model state is invalid.");

            //act
            var result = await sut.UpdateChild(551, null);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateChild_WhenChildIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(Task.FromResult<Child?>(null));

            //act
            var result = await sut.UpdateChild(54, new ChildDTO());
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var data = badRequestResult.Value as APIResponse;
            Assert.NotNull(data);
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("Invalid child ID.", data.Message);
        }

        [Fact]
        public async Task UpdateChild_WhenThereIsAnErrorInAdding_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(new Child());

            A.CallTo(() => childrenService.UpdateChild(A<Child>.Ignored, A<ChildDTO>.Ignored))
                .Returns(new Result { StatusCode = 400, Message = "There is an error." });

            //act
            var result = await sut.UpdateChild(54, new ChildDTO());
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var data = badRequestResult.Value as APIResponse;
            Assert.NotNull(data);
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("There is an error.", data.Message);
        }

        [Fact]
        public async Task UpdateChild_WhenThereIsNotAnyErrors_UpdateChildSuccessfully()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(new Child());

            A.CallTo(() => childrenService.UpdateChild(A<Child>.Ignored, A<ChildDTO>.Ignored))
                .Returns(new Result { StatusCode = 200, Entity = new Child() });

            //act
            var result = await sut.UpdateChild(54, new ChildDTO());
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteChild_WhenChildIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(Task.FromResult<Child?>(null));

            //act
            var result = await sut.DeleteChild(54);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var data = badRequestResult.Value as APIResponse;
            Assert.NotNull(data);
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("Invalid child ID.", data.Message);
        }

        [Fact]
        public async Task DeleteChild_WhenThereIsNotAnyErrors_UpdateChildSuccessfully()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(new Child());

            //act
            var result = await sut.DeleteChild(54);
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var data = okResult.Value as APIResponse;
            Assert.Equal(200, data.StatusCode);
            Assert.Equal("Child deleted.", data.Message);
        }

        [Fact]
        public async Task UpdateChildReadingAndWritingDetails_WhenModelStateIsNotValid_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            sut.ModelState.AddModelError("Custom", "Model state is invalid.");

            //act
            var result = await sut.UpdateChildReadingAndWritingDetails(null, 551);
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task UpdateChildReadingAndWritingDetails_WhenChildIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var childrenService = A.Fake<IChildrenService>();

            var sut = new ChildrenController(childrenService, userManager);

            A.CallTo(() => childrenService.GetChildById(A<int>.Ignored))
                .Returns(Task.FromResult<Child?>(null));

            //act
            var result = await sut.UpdateChildReadingAndWritingDetails(new JsonPatchDocument<Child>(), 551);
            var notFoundResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);

            var data = notFoundResult.Value as APIResponse;
            Assert.NotNull(data);
            Assert.Equal(404, data.StatusCode);
            Assert.Equal("Invalid child ID.", data.Message);
        }
    }
}