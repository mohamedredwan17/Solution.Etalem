using Etalem.Models;

namespace Etalem.Data.Repo.Interfaces
{
    public interface ICourseRepository: IRepository<Course>
    {
        Task<IEnumerable<Course>> GetCoursesWithDetailsAsync();
        Task<Course?> GetCourseWithDetailsAsync(int id);
    }
}
