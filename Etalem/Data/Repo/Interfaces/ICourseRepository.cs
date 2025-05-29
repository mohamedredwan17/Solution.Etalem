using Etalem.Models;

namespace Etalem.Data.Repo.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        // دوال إضافية خاصة بالكورسات لو احتجناها
        IQueryable<Course> GetCourseWithDetailsAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructorId);
        Task<int> GetLessonsCountAsync(int courseId);
        Task<int> GetDurationInMinutesAsync(int courseId);
        Task ExecuteUpdateAsync(string sql, object[] parameters);

    }
}
