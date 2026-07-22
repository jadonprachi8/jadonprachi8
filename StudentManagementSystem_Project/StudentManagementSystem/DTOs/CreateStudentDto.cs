using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.DTOs
{
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Course is required")]
        [MaxLength(100)]
        public string Course { get; set; } = string.Empty;
    }
}
