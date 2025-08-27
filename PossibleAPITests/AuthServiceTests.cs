using FakeItEasy;
using GP_API.Models;
using GP_API.Models.DTOs;
using GP_API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace PossibleAPITests
{
    public class AuthServiceTests
    {
        dynamic _context = new InMemoryDbContext();
        dynamic _environment = A.Fake<IWebHostEnvironment>();
        dynamic _userManager = A.Fake<UserManager<AppUser>>();
        dynamic _configuration = A.Fake<IConfiguration>();
        dynamic _httpContextAccessor = A.Fake<IHttpContextAccessor>();

        [Fact]
        public async Task GenerateToken_WhereUserIsExists_GenerateTokenToUser()
        {
            //arrange
            var inMemorySettings = new Dictionary<string, string>
            {

                { "JWT:SecretKey", "supersecret_test_kjvfjkfjf=====key_123456789" },
                { "JWT:Issuer", "TestIssuer" },
                { "JWT:Audience", "TestAudience" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var sut = new AuthService(_context, _environment, _userManager, configuration, _httpContextAccessor);

            var user = new AppUser()
            {
                UserName = "Test",
                NormalizedUserName = "TEST",
                Email = "Test@mail.com",
                NormalizedEmail = "TEST@MAIL.COM",
                PhoneNumber = "1234567890"
            };

            await _userManager.CreateAsync(user, "123456");

            //act
            var token = await sut.GenerateToken(user);

            //assert
            Assert.NotNull(token);
        }

        [Fact]
        public async Task Update_WhereClientFileIsMissing_ReturnStatusCode400()
        {
            //arrange
            var sut = new AuthService(_context, _environment, _userManager, _configuration, _httpContextAccessor);

            var user = new AppUser()
            {
                UserName = "Test",
                Email = "Test@mail.com"
            };

            await _userManager.CreateAsync(user, "123456");

            var data = new UserDTO()
            {
                UserName = "Updated Name",
                Email = "Updated Email",
                RoleNo = 1,
                Password = "123456"
            };

            //act
            var result = await sut.Update(user, data);

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Client file is missing", result.Message);
        }

        [Fact]
        public async Task Update_WhereAllDataIsValid_UpdateUserSuccessfully()
        {
            //arrange
            var user = new AppUser()
            {
                UserName = "Test",
                Email = "Test@mail.com"
            };

            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.GetRolesAsync(user))
                .Returns(Task.FromResult<IList<string>>(new List<string> { "Admin" }));

            A.CallTo(() => userManager.UpdateAsync(A<AppUser>.Ignored))
                .Returns(IdentityResult.Success);

            var sut = new AuthService(_context, _environment, userManager, _configuration, _httpContextAccessor);

            await _userManager.CreateAsync(user, "123456");

            var content = "Hello World!";
            var fileName = "test.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var data = new UserDTO()
            {
                UserName = "Updated Name",
                Email = "Updated Email",
                RoleNo = 1,
                Password = "123456",
                ClientFile = formFile
            };

            //act
            var result = await sut.Update(user, data);

            //assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Updated Name", result.UserName);
            Assert.Equal("Updated Email", result.Email);
        }

        [Fact]
        public async Task Login_WhereUserDoesNotFound_ReturnStatusCode401()
        {
            //arrange
            var sut = new AuthService(_context, _environment, _userManager, _configuration, _httpContextAccessor);

            //act
            var result = await sut.Login(new LoginDTO() { Email = "Test@mail.com", Password = "123456" });

            //assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Invalid email or password", result.Message);
        }

        [Fact]
        public async Task Login_WherePasswordIsIncorrect_ReturnStatusCode401()
        {
            //arrange
            var sut = new AuthService(_context, _environment, _userManager, _configuration, _httpContextAccessor);

            var user = new AppUser()
            {
                UserName = "Test",
                NormalizedUserName = "TEST",
                Email = "Test@mail.com",
                NormalizedEmail = "TEST@MAIL.COM",
                PhoneNumber = "1234567890"
            };

            await _userManager.CreateAsync(user, "123456");

            //act
            var result = await sut.Login(new LoginDTO() { Email = "Test@mail.com", Password = "165456" });

            //assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Invalid email or password", result.Message);
        }

        [Fact]
        public async Task Login_WhereUserFoundAndPasswordCorrect_LoginSuccessfully()
        {
            //arrange
            var inMemorySettings = new Dictionary<string, string>
            {

                { "JWT:SecretKey", "supersecret_test_kjvfjkfjf=====key_123456789" },
                { "JWT:Issuer", "TestIssuer" },
                { "JWT:Audience", "TestAudience" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var userManager = A.Fake<UserManager<AppUser>>();

            var user = new AppUser()
            {
                UserName = "Test",
                NormalizedUserName = "TEST",
                Email = "Test@mail.com",
                NormalizedEmail = "TEST@MAIL.COM",
                PhoneNumber = "1234567890"
            };

            await userManager.CreateAsync(user, "123456");

            A.CallTo(() => userManager.FindByEmailAsync(user.Email))
             .Returns(user);

            A.CallTo(() => userManager.CheckPasswordAsync(user, "123456"))
               .Returns(true);

            A.CallTo(() => userManager.GetRolesAsync(user))
            .Returns(new List<string>() { "User" });

            var sut = new AuthService(_context, _environment, userManager, configuration, _httpContextAccessor);

            //act
            var result = await sut.Login(new LoginDTO() { Email = "Test@mail.com", Password = "123456" });

            //assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task Register_WhereClientFileIsMissing_ReturnStatusCode400()
        {
            //arrange
            var sut = new AuthService(_context, _environment, _userManager, _configuration, _httpContextAccessor);

            var data = new UserDTO()
            {
                UserName = "Updated Name",
                Email = "Updated Email",
                RoleNo = 1,
                Password = "123456"
            };

            //act
            var result = await sut.Register(data);

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Client file is missing.", result.Message);
        }

        [Fact]
        public async Task Register_WhereRoleNumberIsInvalid_ReturnStatusCode400()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();

            var sut = new AuthService(_context, _environment, userManager, _configuration, _httpContextAccessor);

            var content = "Hello World!";
            var fileName = "test.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var data = new UserDTO()
            {
                UserName = "Test",
                Email = "Test@mail.com",
                RoleNo = 10,
                Password = "123456",
                ClientFile = formFile
            };

            A.CallTo(() => userManager.CreateAsync(A<AppUser>.Ignored, A<string>.Ignored))
               .Returns(IdentityResult.Success);

            //act
            var result = await sut.Register(data);

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("RoleNo must be 1 or 2.", result.Message);
        }

        [Fact]
        public async Task Register_WhereDataIsValid_RegisterSuccessfully()
        {
            //arrange
            var inMemorySettings = new Dictionary<string, string>
            {

                { "JWT:SecretKey", "supersecret_test_kjvfjkfjf=====key_123456789" },
                { "JWT:Issuer", "TestIssuer" },
                { "JWT:Audience", "TestAudience" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var userManager = A.Fake<UserManager<AppUser>>();

            var sut = new AuthService(_context, _environment, userManager, configuration, _httpContextAccessor);

            var content = "Hello World!";
            var fileName = "test.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var user = new UserDTO()
            {
                UserName = "Updated Name",
                Email = "Updated Email",
                RoleNo = 1,
                Password = "123456",
                ClientFile = formFile
            };

            A.CallTo(() => userManager.CreateAsync(A<AppUser>.Ignored, A<string>.Ignored))
                .Returns(IdentityResult.Success);

            A.CallTo(() => userManager.AddToRoleAsync(A<AppUser>.Ignored, A<string>.Ignored))
                .Returns(IdentityResult.Success);

            A.CallTo(() => userManager.GetRolesAsync(A<AppUser>.Ignored))
                .Returns(new List<string>() { "User" });

            //act
            var result = await sut.Register(user);

            //assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("User", result.Roles.First());
            Assert.NotEmpty(result.Token);
        }
    }
}
