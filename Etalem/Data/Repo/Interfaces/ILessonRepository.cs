using Etalem.Models;

namespace Etalem.Data.Repo.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId);
        IQueryable<Lesson> GetLessonWithResourcesAsync(int lessonId);
    }
}
