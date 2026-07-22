using StudentManagementSystem.DTOs;
using StudentManagementSystem.Exceptions;
using StudentManagementSystem.Models;
using StudentManagementSystem.Repositories;

namespace StudentManagementSystem.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository repository, ILogger<StudentService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
        {
            _logger.LogInformation("Fetching all students");
            var students = await _repository.GetAllAsync();
            return students.Select(MapToDto);
        }

        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student with id {Id} not found", id);
                throw new NotFoundException($"Student with id {id} was not found.");
            }

            return MapToDto(student);
        }

        public async Task<StudentDto> AddStudentAsync(CreateStudentDto dto)
        {
            var existing = await _repository.GetByEmailAsync(dto.Email);
            if (existing != null)
            {
                _logger.LogWarning("Attempt to add student with duplicate email {Email}", dto.Email);
                throw new BadRequestException($"A student with email '{dto.Email}' already exists.");
            }

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Age = dto.Age,
                Course = dto.Course,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(student);
            _logger.LogInformation("Created new student with id {Id}", created.Id);
            return MapToDto(created);
        }

        public async Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Attempted to update non-existent student with id {Id}", id);
                throw new NotFoundException($"Student with id {id} was not found.");
            }

            var existingWithEmail = await _repository.GetByEmailAsync(dto.Email);
            if (existingWithEmail != null && existingWithEmail.Id != id)
            {
                throw new BadRequestException($"Another student already uses email '{dto.Email}'.");
            }

            student.Name = dto.Name;
            student.Email = dto.Email;
            student.Age = dto.Age;
            student.Course = dto.Course;

            var updated = await _repository.UpdateAsync(student);
            _logger.LogInformation("Updated student with id {Id}", id);
            return MapToDto(updated);
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Attempted to delete non-existent student with id {Id}", id);
                throw new NotFoundException($"Student with id {id} was not found.");
            }

            await _repository.DeleteAsync(student);
            _logger.LogInformation("Deleted student with id {Id}", id);
        }

        private static StudentDto MapToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Age = student.Age,
                Course = student.Course,
                CreatedDate = student.CreatedDate
            };
        }
    }
}
