using GP_API.DTOs;
using GP_API.Helpers;
using GP_API.Models;
using GP_API.Repository.Interfaces;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GP_API.Services
{
    public class ChildrenService : IChildrenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChildrenService(UserManager<AppUser> userManager, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Child>> GetAllChildren()
        {
            return await _unitOfWork.Children.GetAll();
        }

        public async Task<Child> GetChildById(int id)
        {
            return await _unitOfWork.Children.Get(id);
        }

        public async Task<IEnumerable<Child>> GetChildrenByParentId(string id)
        {
            return await _unitOfWork.Children.GetParentChildren(id);
        }

        public async Task<Result> AddChild(ChildDTO data)
        {
            var parent = await _userManager.FindByNameAsync(data.ParentUserName);

            if (parent == null)
                return new Result() { Message = "Invalid parent name !!", StatusCode = 400 };

            var webRootPath = _environment.WebRootPath;

            if (data.ClientFile == null)
            {
                return new Result() { Message = "Client file is missing", StatusCode = 400 };
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(data.ClientFile.FileName);
            var filePath = Path.Combine(webRootPath, "files/uploads/images", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await data.ClientFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return new Result() { StatusCode = 500, Message = "Error saving file: " + ex.Message };
            }

            var request = _httpContextAccessor.HttpContext.Request;

            var child = new Child()
            {
                Name = data.Name,
                Age = data.Age,
                ParentId = parent.Id,
                Difficult = data.Difficult,
                Gender = data.Gender,
                Image = $"{request.Scheme}://{request.Host}/files/uploads/images/{fileName}"
            };

            var result = await _unitOfWork.Children.Add(child);
            await _unitOfWork.Commit();

            return result is not null ? new Result { StatusCode = 200, Entity = child } :
              new Result { StatusCode = 400, Message = "An error occured whild adding." };
        }

        public async Task<Result> UpdateChild(Child child, ChildDTO data)
        {
            var parent = await _userManager.FindByNameAsync(data.ParentUserName);

            if (parent == null)
                return new Result() { Message = "Invalid parent name !!", StatusCode = 400 };

            var webRootPath = _environment.WebRootPath;

            if (data.ClientFile == null)
            {
                return new Result() { Message = "Client file is missing", StatusCode = 400 };
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(data.ClientFile.FileName);
            var filePath = Path.Combine(webRootPath, "files/uploads/images", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await data.ClientFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return new Result() { StatusCode = 500, Message = "Error saving file: " + ex.Message };
            }

            var request = _httpContextAccessor.HttpContext.Request;

            child.Name = data.Name;
            child.Age = data.Age;
            child.Gender = data.Gender;
            child.Difficult = data.Difficult;
            child.ParentId = parent.Id;
            child.Image = $"{request.Scheme}://{request.Host}/files/uploads/images/{fileName}";

            await _unitOfWork.Commit();

            return new Result()
            {
                StatusCode = 200,
                Entity = child
            };
        }

        public async Task<Result> DeleteChild(Child child)
        {
            var result = _unitOfWork.Children.Delete(child);

            await _unitOfWork.Commit();

            return result is not null ? new Result { StatusCode = 200, Entity = child } :
                new Result { StatusCode = 400, Message = "An error occured whild deleting." };
        }

        public async Task SaveChanges()
        {
            await _unitOfWork.Commit();
        }
    }
}
