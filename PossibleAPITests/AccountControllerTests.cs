using FakeItEasy;
using GP_API.Controllers;
using GP_API.DTOs;
using GP_API.Models;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PossibleAPITests
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Register_WhenThereIsIssue_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            A.CallTo(() => authService.Register(A<UserDTO>.Ignored))
                .Returns(new AuthModel() { StatusCode = 400, Message = "There is an error." });

            //act
            var result = await sut.Register(new UserDTO());
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("There is an error.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task Register_WhenDataIsValid_RegisterSuccessfully()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            A.CallTo(() => authService.Register(A<UserDTO>.Ignored))
                .Returns(new AuthModel() { StatusCode = 200 });

            //act
            var result = await sut.Register(new UserDTO());
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as APIResponse;
            Assert.Equal("User registered.", data.Message);
            Assert.Equal(200, data.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_WhenUserIDIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

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
            var result = await sut.UpdateUser(new UserDTO());
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
        public async Task UpdateUser_WhenUserIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

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
            var result = await sut.UpdateUser(new UserDTO());
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
        public async Task UpdateUser_WhenThereIsAnIssueWhileUpdate_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(new AppUser());

            A.CallTo(() => authService.Update(A<AppUser>.Ignored, A<UserDTO>.Ignored))
                .Returns(new AuthModel { StatusCode = 400, Message = "There is an error." });

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
            var result = await sut.UpdateUser(new UserDTO());
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal("There is an error.", data.Message);
            Assert.Equal(400, data.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_WhenAllDataIsValid_UpdateSuccessfully()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(new AppUser());

            A.CallTo(() => authService.Update(A<AppUser>.Ignored, A<UserDTO>.Ignored))
                .Returns(new AuthModel { StatusCode = 200 });

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
            var result = await sut.UpdateUser(new UserDTO());
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as APIResponse;
            Assert.Equal("User updated.", data.Message);
            Assert.Equal(200, data.StatusCode);
        }

        [Fact]
        public async Task Login_WhenThereIsAnError_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            A.CallTo(() => authService.Login(A<LoginDTO>.Ignored))
                .Returns(new AuthModel { Message = "There is an error.", StatusCode = 400 });

            var sut = new AccountController(tokenService, authService, userManager);

            //act 
            var result = await sut.Login(new LoginDTO());
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as APIResponse;
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("There is an error.", data.Message);
        }

        [Fact]
        public async Task Login_WhenThereIsNotAnErrors_LoginSuccessfully()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            A.CallTo(() => authService.Login(A<LoginDTO>.Ignored))
                .Returns(new AuthModel { StatusCode = 200 });

            var sut = new AccountController(tokenService, authService, userManager);

            //act 
            var result = await sut.Login(new LoginDTO());
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as APIResponse;
            Assert.Equal(200, data.StatusCode);
            Assert.Equal("User logged in.", data.Message);
        }

        [Fact]
        public async Task Logout_WhenUserIDIsNull_ReturnBadRequest()
        {
            //arrange 
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            //act 
            var result = await sut.Logout();
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("Can't find userID.", data.Message);
        }

        [Fact]
        public async Task Logout_WhenUserIDIsFoundAndThereIsNoIssues_LogoutSuccessfully()
        {
            //arrange 
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

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
            var result = await sut.Logout();
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var data = okResult.Value as APIResponse;
            Assert.Equal(200, data.StatusCode);
            Assert.Equal("Logged out successfully.", data.Message);
        }

        [Fact]
        public async Task GetUserData_WhenUserIDIsNull_ReturnBadRequest()
        {
            //arrange 
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            sut.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            //act 
            var result = await sut.GetUserData();
            var badRequestResult = result as ObjectResult;

            //assert
            Assert.NotNull(result);
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var data = badRequestResult.Value as APIResponse;
            Assert.Equal(400, data.StatusCode);
            Assert.Equal("Invalid token.", data.Message);
        }

        [Fact]
        public async Task GetUserData_WhenUserIsNull_ReturnBadRequest()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

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
            var result = await sut.GetUserData();
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
        public async Task GetUserData_WhenThereIsNotAnyIssues_ReturnUserData()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();
            var tokenService = A.Fake<ITokenService>();
            var authService = A.Fake<IAuthService>();

            var sut = new AccountController(tokenService, authService, userManager);

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(Task.FromResult<AppUser?>(null));

            A.CallTo(() => userManager.FindByIdAsync(A<string>.Ignored))
                .Returns(new AppUser
                {
                    Id = "#1",
                    Email = "Email",
                    UserName = "UserName",
                    Image = "Image",
                });

            A.CallTo(() => userManager.GetRolesAsync(A<AppUser>.Ignored))
               .Returns(new List<string> { "User" });

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
            var result = await sut.GetUserData();
            var okResult = result as ObjectResult;

            //assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);

            var data = okResult.Value as UserViewDTO;
            Assert.Equal("#1", data.Id);
            Assert.Equal("Email", data.Email);
            Assert.Equal("UserName", data.UserName);
        }
    }
}
