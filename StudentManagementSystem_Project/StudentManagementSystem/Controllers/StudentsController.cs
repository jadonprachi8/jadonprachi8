using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>Get all students.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return Ok(ApiResponse<IEnumerable<StudentDto>>.SuccessResponse(students));
        }

        /// <summary>Get a single student by id.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            return Ok(ApiResponse<StudentDto>.SuccessResponse(student));
        }

        /// <summary>Add a new student.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed."));
            }

            var created = await _studentService.AddStudentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<StudentDto>.SuccessResponse(created, "Student created successfully"));
        }

        /// <summary>Update an existing student.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed."));
            }

            var updated = await _studentService.UpdateStudentAsync(id, dto);
            return Ok(ApiResponse<StudentDto>.SuccessResponse(updated, "Student updated successfully"));
        }

        /// <summary>Delete a student.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _studentService.DeleteStudentAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Student deleted successfully"));
        }
    }
}
