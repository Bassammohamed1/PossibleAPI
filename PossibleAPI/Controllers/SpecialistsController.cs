using GP_API.Data;
using GP_API.Models;
using GP_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialistsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpecialistsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetChildTests/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetChildTests(int id)
        {
            var testIds = await _context.TestChildren.Where(t => t.ChildId == id).Select(t => t.TestId).Distinct().ToListAsync();

            var testData = new List<TestDTO>();

            foreach (var testId in testIds)
            {
                var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);

                if (test != null)
                {
                    var questions = await _context.Questions.Where(q => q.TestId == testId).ToListAsync();

                    var testDTO = new TestDTO()
                    {
                        TestName = test.Name,
                        TestCategory = test.Category,
                        QuestionsNo = test.QuestionNo,
                        Questions = questions.Select(q => new QuestionDTO
                        {
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            QuestionAnswer = q.QuestionAnswer
                        }).ToList()
                    };

                    testData.Add(testDTO);
                }
            }

            return Ok(testData);
        }

        [HttpPost("CreateTest")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> CreateTest([FromBody] CreateTestDTO testDTO)
        {
            if (string.IsNullOrEmpty(testDTO.TestName) || string.IsNullOrEmpty(testDTO.TestCategory))
                return BadRequest(new APIResponse { Message = "Test name or test category is null.", StatusCode = 400 });

            if (!testDTO.Questions.Any() || !testDTO.ChildrenId.Any())
                return BadRequest(new APIResponse { Message = "Questions or childrenIDs can't be empty.", StatusCode = 400 });

            var test = new Test()
            {
                Name = testDTO.TestName,
                Category = testDTO.TestCategory,
                QuestionNo = testDTO.QuestionsNo
            };

            await _context.Tests.AddAsync(test);
            await _context.SaveChangesAsync();

            foreach (var childId in testDTO.ChildrenId)
            {
                var data1 = new TestChildren()
                {
                    ChildId = childId,
                    TestId = test.Id
                };
                await _context.TestChildren.AddAsync(data1);
            }
            await _context.SaveChangesAsync();

            foreach (var question in testDTO.Questions)
            {
                var data2 = new Question()
                {
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType,
                    QuestionAnswer = question.QuestionAnswer,
                    TestId = test.Id,
                };
                await _context.Questions.AddAsync(data2);
            }
            await _context.SaveChangesAsync();

            return Ok(new APIResponse { Message = "Test has been created successfully.", StatusCode = 200 });
        }
    }
}
