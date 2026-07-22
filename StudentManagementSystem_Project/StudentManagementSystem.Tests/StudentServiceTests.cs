using Microsoft.Extensions.Logging;
using Moq;
using StudentManagementSystem.DTOs;
using StudentManagementSystem.Exceptions;
using StudentManagementSystem.Models;
using StudentManagementSystem.Repositories;
using StudentManagementSystem.Services;
using Xunit;

namespace StudentManagementSystem.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repoMock;
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _repoMock = new Mock<IStudentRepository>();
            var loggerMock = new Mock<ILogger<StudentService>>();
            _service = new StudentService(_repoMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ReturnsMappedStudents()
        {
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Student>
            {
                new Student { Id = 1, Name = "Alice", Email = "alice@test.com", Age = 20, Course = "CS" }
            });

            var result = await _service.GetAllStudentsAsync();

            Assert.Single(result);
            Assert.Equal("Alice", result.First().Name);
        }

        [Fact]
        public async Task GetStudentByIdAsync_ThrowsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetStudentByIdAsync(99));
        }

        [Fact]
        public async Task AddStudentAsync_ThrowsBadRequest_WhenEmailAlreadyExists()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("dup@test.com"))
                .ReturnsAsync(new Student { Id = 1, Email = "dup@test.com" });

            var dto = new CreateStudentDto { Name = "Bob", Email = "dup@test.com", Age = 21, Course = "IT" };

            await Assert.ThrowsAsync<BadRequestException>(() => _service.AddStudentAsync(dto));
        }

        [Fact]
        public async Task AddStudentAsync_CreatesStudent_WhenEmailIsUnique()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("new@test.com")).ReturnsAsync((Student?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Student>()))
                .ReturnsAsync((Student s) => { s.Id = 5; return s; });

            var dto = new CreateStudentDto { Name = "Carol", Email = "new@test.com", Age = 22, Course = "Math" };

            var result = await _service.AddStudentAsync(dto);

            Assert.Equal(5, result.Id);
            Assert.Equal("Carol", result.Name);
        }

        [Fact]
        public async Task DeleteStudentAsync_ThrowsNotFound_WhenMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Student?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteStudentAsync(42));
        }

        [Fact]
        public async Task UpdateStudentAsync_UpdatesFields_WhenValid()
        {
            var existing = new Student { Id = 1, Name = "Old", Email = "old@test.com", Age = 20, Course = "CS" };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.GetByEmailAsync("updated@test.com")).ReturnsAsync((Student?)null);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Student>())).ReturnsAsync((Student s) => s);

            var dto = new UpdateStudentDto { Name = "New", Email = "updated@test.com", Age = 25, Course = "Physics" };

            var result = await _service.UpdateStudentAsync(1, dto);

            Assert.Equal("New", result.Name);
            Assert.Equal("updated@test.com", result.Email);
            Assert.Equal(25, result.Age);
        }
    }
}
