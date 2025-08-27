using GP_API.Data;
using GP_API.Models;
using GP_API.Models.DTOs;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GP_API.Services
{
    public class ChildrenService : IChildrenService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChildrenService(AppDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Child>> GetAllChildren()
        {
            return await _context.Children.ToListAsync();
        }

        public async Task<Child> GetChildById(int id)
        {
            var child = await _context.Children.FindAsync(id);
            return child;
        }

        public async Task<List<Child>> GetChildrenByParentId(string id)
        {
            var children = await _context.Children.Where(x => x.ParentId == id).ToListAsync();
            return children;
        }

        public async Task<DbOperationModel> AddChild(ChildDTO data)
        {
            var parent = await _userManager.FindByNameAsync(data.ParentUserName);

            if (parent == null)
                return new DbOperationModel() { Message = "Invalid parent name !!", StatusCode = 400 };

            var webRootPath = _environment.WebRootPath;

            if (data.ClientFile == null)
            {
                return new DbOperationModel() { Message = "Client file is missing", StatusCode = 400 };
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
                return new DbOperationModel() { StatusCode = 500, Message = "Error saving file: " + ex.Message };
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

            await _context.AddAsync(child);
            await _context.SaveChangesAsync();

            return new DbOperationModel()
            {
                StatusCode = 200,
                Child = child
            };
        }

        public async Task<DbOperationModel> UpdateChild(Child child, ChildDTO data)
        {
            var parent = await _userManager.FindByNameAsync(data.ParentUserName);

            if (parent == null)
                return new DbOperationModel() { Message = "Invalid parent name !!", StatusCode = 400 };

            var webRootPath = _environment.WebRootPath;

            if (data.ClientFile == null)
            {
                return new DbOperationModel() { Message = "Client file is missing", StatusCode = 400 };
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
                return new DbOperationModel() { StatusCode = 500, Message = "Error saving file: " + ex.Message };
            }

            var request = _httpContextAccessor.HttpContext.Request;

            child.Name = data.Name;
            child.Age = data.Age;
            child.Gender = data.Gender;
            child.Difficult = data.Difficult;
            child.ParentId = parent.Id;
            child.Image = $"{request.Scheme}://{request.Host}/files/uploads/images/{fileName}";

            await _context.SaveChangesAsync();

            return new DbOperationModel()
            {
                StatusCode = 200,
                Child = child
            };
        }

        public async Task DeleteChild(Child child)
        {
            _context.Children.Remove(child);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
