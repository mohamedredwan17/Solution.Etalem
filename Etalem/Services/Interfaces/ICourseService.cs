using Etalem.Models.DTOs.Course;

namespace Etalem.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetCoursesByInstructorIdAsync(string instructorId);

        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task CreateCourseAsync(CourseCreateDto courseDto, List<int> categoryIds, string instructorId);

        Task UpdateCourseAsync(int id, CourseUpdateDto dto, List<int> categoryIds, string instructorId);
        Task<bool> DeleteCourseAsync(int id);
    }
}
