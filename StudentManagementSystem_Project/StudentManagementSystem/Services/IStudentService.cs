using StudentManagementSystem.DTOs;

namespace StudentManagementSystem.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
        Task<StudentDto> GetStudentByIdAsync(int id);
        Task<StudentDto> AddStudentAsync(CreateStudentDto dto);
        Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentDto dto);
        Task DeleteStudentAsync(int id);
    }
}
