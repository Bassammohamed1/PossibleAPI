using GP_API.DTOs;
using GP_API.Models;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildrenController : ControllerBase
    {
        private readonly IChildrenService _childrenService;
        private readonly UserManager<AppUser> _userManager;

        public ChildrenController(IChildrenService childrenService, UserManager<AppUser> userManager)
        {
            _childrenService = childrenService;
            _userManager = userManager;
        }

        [HttpGet("GetAllChildren")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> GetAllChildren()
        {
            var children = await _childrenService.GetAllChildren();

            return Ok(children);
        }

        [HttpGet("GetChildById/{id}")]
        [Authorize(Roles = "User,Specialist")]
        public async Task<IActionResult> GetChildById(int id)
        {
            if (id == 0 || id == null)
                return BadRequest(new APIResponse { Message = "Invalid id.", StatusCode = 400 });

            var child = await _childrenService.GetChildById(id);

            if (child == null)
                return NotFound(new APIResponse { Message = "Child not found.", StatusCode = 404 });

            return Ok(new
            {
                Id = child.Id,
                Name = child.Name,
                Age = child.Age,
                Difficult = child.Difficult,
                ReadingDays = child.ReadingDays ?? 0,
                WritingDays = child.WritingDays ?? 0,
                LastReadingTime = child.LastReadingTime,
                LastWritingTime = child.LastWritingTime,
                ReadingRate = child.ReadingRate ?? 0,
                WritingRate = child.WritingRate ?? 0,
                ParentId = child.ParentId,
                Gender = child.Gender,
                Image = child.Image
            });
        }

        [HttpGet("GetUserChildren")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserChildren()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new APIResponse { Message = "Invalid token.", StatusCode = 400 });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new APIResponse { Message = "User not found.", StatusCode = 400 });

            var children = await _childrenService.GetChildrenByParentId(userId);
            if (children == null)
                return BadRequest(new APIResponse { Message = "Invalid parent ID.", StatusCode = 400 });

            var data = new List<ChildViewDTO>();

            foreach (var child in children)
            {
                data.Add(new ChildViewDTO
                {
                    Id = child.Id,
                    Name = child.Name,
                    Age = child.Age,
                    Gender = child.Gender,
                    ParentUserName = user.UserName,
                    Difficult = child.Difficult,
                    ReadingDays = child.ReadingDays ?? 0,
                    WritingDays = child.WritingDays ?? 0,
                    LastReadingTime = child.LastReadingTime,
                    LastWritingTime = child.LastWritingTime,
                    ReadingRate = child.ReadingRate ?? 0,
                    WritingRate = child.WritingRate ?? 0,
                    Image = child.Image
                });
            }

            return Ok(data);
        }

        [HttpPost("AddChild")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddChild([FromForm] ChildDTO data)
        {
            if (ModelState.IsValid)
            {
                var result = await _childrenService.AddChild(data);

                if (result.StatusCode == 200)
                {
                    return Ok(result.Entity);
                }
                else
                {
                    return BadRequest(new APIResponse { StatusCode = result.StatusCode, Message = result.Message });
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPut("UpdateChild/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateChild(int id, [FromForm] ChildDTO data)
        {
            if (ModelState.IsValid)
            {
                var child = await _childrenService.GetChildById(id);

                if (child is null)
                    return BadRequest(new APIResponse { Message = "Invalid child ID.", StatusCode = 400 });

                var result = await _childrenService.UpdateChild(child, data);

                if (result.StatusCode == 200)
                {
                    return Ok(result.Entity);
                }
                else
                {
                    return BadRequest(new APIResponse { StatusCode = result.StatusCode, Message = result.Message });
                }
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("DeleteChild/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteChild(int id)
        {
            var child = await _childrenService.GetChildById(id);
            if (child is null)
                return BadRequest(new APIResponse { Message = "Invalid child ID.", StatusCode = 400 });

            _childrenService.DeleteChild(child);

            return Ok(new APIResponse { Message = "Child deleted.", StatusCode = 200 });
        }

        [HttpPatch("UpdateChildReadingAndWritingDetails/{id}")]
        [Authorize(Roles = "User,Specialist")]
        public async Task<IActionResult> UpdateChildReadingAndWritingDetails([FromBody] JsonPatchDocument<Child> data, int id)
        {
            if (ModelState.IsValid)
            {
                var child = await _childrenService.GetChildById(id);

                if (child != null)
                {
                    data.ApplyTo(child);
                    await _childrenService.SaveChanges();
                    return Ok(child);
                }
                else
                    return NotFound(new APIResponse { Message = "Invalid child ID.", StatusCode = 404 });
            }
            else
                return BadRequest(ModelState);
        }
    }
}