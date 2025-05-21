using Etalem.Models;

namespace Etalem.Data.Repo.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        // دوال إضافية خاصة بالكورسات لو احتجناها
        Task<Course> GetCourseWithDetailsAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructorId);
    }
}
