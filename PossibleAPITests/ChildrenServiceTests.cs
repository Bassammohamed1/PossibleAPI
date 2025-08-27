using FakeItEasy;
using GP_API.Data;
using GP_API.Models;
using GP_API.Models.DTOs;
using GP_API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace PossibleAPITests
{
    public class ChildrenServiceTests
    {
        dynamic _context = new InMemoryDbContext();
        dynamic _userManager = A.Fake<UserManager<AppUser>>();
        dynamic _environment = A.Fake<IWebHostEnvironment>();
        dynamic _httpContextAccessor = A.Fake<IHttpContextAccessor>();

        [Fact]
        public async Task GetAllChildren_WhenThereIsNoChildren_ReturnEmptyEnumerable()
        {
            //arrange
            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            //act
            var result = await sut.GetAllChildren();

            //assert
            Assert.Empty(result);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAllChildren_WhenThereIsChildren_ReturnChildren()
        {
            //arrange
            var children = new List<Child>()
            {
                new Child(){
                    Name = "Child 1",
                    Age = 3,
                    Gender = Gender.Male,
                    Image = "Image path #1",
                    Difficult = "Child difficult",
                    ParentId = "Parent ID"
                },
                new Child(){
                    Name = "Child 2",
                    Age = 4,
                    Gender = Gender.Male,
                    Image = "Image path #2",
                    Difficult = "Child difficult",
                    ParentId = "Parent ID"
                },
                new Child(){
                    Name = "Child 3",
                    Age = 5,
                    Gender = Gender.Female,
                    Image = "Image path #3",
                    Difficult = "Child difficult",
                    ParentId = "Parent ID"
                }
            };

            await _context.Children.AddRangeAsync(children);
            await _context.SaveChangesAsync();

            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            //act
            var result = await sut.GetAllChildren();

            //assert
            Assert.True(result.Any());
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetChildById_WhenChildIsNotExists_ReturnNull()
        {
            //arrange
            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            //act
            var result = await sut.GetChildById(1);

            //assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetChildById_WhenChildIsExists_ReturnChild()
        {
            //arrange
            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            var child = new Child()
            {
                Name = "Child 1",
                Age = 3,
                Gender = Gender.Male,
                Image = "Image path #1",
                Difficult = "Child difficult",
                ParentId = "Parent ID"
            };

            await _context.Children.AddAsync(child);
            await _context.SaveChangesAsync();

            //act
            var result = await sut.GetChildById(child.Id);

            //assert
            Assert.NotNull(result);
            Assert.Equal("Child 1", result.Name);
        }

        [Fact]
        public async Task GetChildrenByParentId_WhenThereIsNoChildren_ReturnEmptyEnumerable()
        {
            //arrange
            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            //act 
            var result = await sut.GetChildrenByParentId("Parent ID");

            //assert
            Assert.Empty(result);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetChildrenByParentId_WhenThereIsChildren_ReturnChildren()
        {
            //arrange
            var children = new List<Child>()
            {
                new Child(){
                    Name = "Child 1",
                    Age = 3,
                    Gender = Gender.Male,
                    Image = "Image path #1",
                    Difficult = "Child difficult",
                    ParentId = "Parent ID #1"
                },
                new Child(){
                    Name = "Child 2",
                    Age = 4,
                    Gender = Gender.Male,
                    Image = "Image path #2",
                    Difficult = "Child difficult",
                    ParentId = "Parent ID #1"
                },
                new Child(){
                    Name = "Child 3",
                    Age = 5,
                    Gender = Gender.Female,
                    Image = "Image path #3",
                    Difficult = "Child difficult",
                    ParentId = "Parent ID #2"
                }
            };

            await _context.Children.AddRangeAsync(children);
            await _context.SaveChangesAsync();

            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            //act
            var result = await sut.GetChildrenByParentId("Parent ID #1");

            //assert
            Assert.True(result.Any());
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task DeleteChild_WhenChildIsNotExists_DeleteChild()
        {
            //arrange
            var sut = new ChildrenService(_context, _userManager, _environment, _httpContextAccessor);

            var child = new Child()
            {
                Name = "Child 1",
                Age = 3,
                Gender = Gender.Male,
                Image = "Image path #1",
                Difficult = "Child difficult",
                ParentId = "Parent ID"
            };

            await _context.Children.AddAsync(child);
            await _context.SaveChangesAsync();

            //act
            await sut.DeleteChild(child);

            //assert
            Assert.Null(_context.Children.Find(child.Id));
        }

        [Fact]
        public async Task AddChild_WhenParentIsNotFound_ReturnStatusCode400()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.FindByNameAsync(A<string>.Ignored))
                .Returns(Task.FromResult<AppUser?>(null));

            var sut = new ChildrenService(_context, userManager, _environment, _httpContextAccessor);

            //act 
            var result = await sut.AddChild(new ChildDTO());

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid parent name !!", result.Message);
        }

        [Fact]
        public async Task AddChild_WhenClientFileIsNotFound_ReturnStatusCode400()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.FindByNameAsync(A<string>.Ignored))
                .Returns(new AppUser());

            var sut = new ChildrenService(_context, userManager, _environment, _httpContextAccessor);

            //act 
            var result = await sut.AddChild(new ChildDTO());

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Client file is missing", result.Message);
        }

        [Fact]
        public async Task AddChild_WhenAllDataIsValid_AddChild()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.FindByNameAsync(A<string>.Ignored))
                .Returns(new AppUser());

            var sut = new ChildrenService(_context, userManager, _environment, _httpContextAccessor);

            var content = "Hello World!";
            var fileName = "test.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var child = new ChildDTO()
            {
                Name = "Test",
                Age = 3,
                Difficult = "Child's difficult",
                Gender = Gender.Male,
                ParentUserName = "Parent's name",
                ClientFile = formFile
            };

            //act 
            var result = await sut.AddChild(child);

            //assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Child);
            Assert.True(result.Child?.Id > 0);
        }

        [Fact]
        public async Task UpdateChild_WhenParentIsNotFound_ReturnStatusCode400()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.FindByNameAsync(A<string>.Ignored))
                .Returns(Task.FromResult<AppUser?>(null));

            var sut = new ChildrenService(_context, userManager, _environment, _httpContextAccessor);

            //act 
            var result = await sut.UpdateChild(new Child(), new ChildDTO());

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid parent name !!", result.Message);
        }

        [Fact]
        public async Task UpdateChild_WhenClientFileIsNotFound_ReturnStatusCode400()
        {
            //arrange
            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.FindByNameAsync(A<string>.Ignored))
                .Returns(new AppUser());

            var sut = new ChildrenService(_context, userManager, _environment, _httpContextAccessor);

            //act 
            var result = await sut.UpdateChild(new Child(), new ChildDTO());

            //assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Client file is missing", result.Message);
        }

        [Fact]
        public async Task UpdateChild_WhenAllDataIsValid_AddChild()
        {
            //arrange
            var context = new InMemoryDbContext();
            var userManager = A.Fake<UserManager<AppUser>>();

            A.CallTo(() => userManager.FindByNameAsync(A<string>.Ignored))
                .Returns(new AppUser());

            var sut = new ChildrenService(_context, userManager, _environment, _httpContextAccessor);

            var content = "Hello World!";
            var fileName = "test.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var child = new Child()
            {
                Name = "Test",
                Age = 3,
                Gender = Gender.Male,
                Difficult = "Child's difficult",
                Image = "Child's image",
                ParentId = "Parent's ID"
            };

            await context.Children.AddAsync(child);
            await context.SaveChangesAsync();

            var updatingChild = new ChildDTO()
            {
                Name = "Updated name",
                Age = 4,
                Difficult = "Child's difficult",
                Gender = Gender.Male,
                ParentUserName = "Parent's name",
                ClientFile = formFile
            };

            //act 
            var result = await sut.UpdateChild(child, updatingChild);

            //assert
            Assert.NotNull(result);
            Assert.NotNull(result.Child);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Updated name", result.Child.Name);
            Assert.Equal(4, result.Child.Age);
        }
    }
}
