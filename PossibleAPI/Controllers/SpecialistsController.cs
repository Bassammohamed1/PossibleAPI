using GP_API.DTOs;
using GP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialistsController : ControllerBase
    {
        private readonly ISpecialistsService _specialistsService;

        public SpecialistsController(ISpecialistsService specialistsService)
        {
            _specialistsService = specialistsService;
        }

        [HttpGet("GetChildTests/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetChildTests(int id)
        {
            var result = await _specialistsService.GetChildTests(id);

            return Ok(result);
        }

        [HttpPost("CreateTest")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> CreateTest([FromBody] CreateTestDTO testDTO)
        {
            if (string.IsNullOrEmpty(testDTO.TestName) || string.IsNullOrEmpty(testDTO.TestCategory))
                return BadRequest(new APIResponse { Message = "Test name or test category is null.", StatusCode = 400 });

            if (!testDTO.Questions.Any() || !testDTO.ChildrenId.Any())
                return BadRequest(new APIResponse { Message = "Questions or childrenIDs can't be empty.", StatusCode = 400 });

            var result = await _specialistsService.CreateTest(testDTO);

            return result.StatusCode == 200 ? Ok(new APIResponse { Message = "Test has been created successfully.", StatusCode = 200 }) :
                BadRequest(new APIResponse { Message = result.Message, StatusCode = result.StatusCode });
        }
    }
}
