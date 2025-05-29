using Etalem.Data.Repo.Interfaces;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Etalem.Data.Repo
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        private readonly ApplicationDbContext _context;

        public LessonRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Lesson> GetByIdAsync(int id)
        {
            return await _context.Lessons
                .Include(l => l.Resources)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public override async Task<IEnumerable<Lesson>> GetAllAsync()
        {
            return await _context.Lessons
                .Include(l => l.Resources)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Lesson>> FindAsync(Expression<Func<Lesson, bool>> predicate)
        {
            return await _context.Lessons
                .Include(l => l.Resources)
                .Where(predicate)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId)
        {
            return await _context.Lessons
                .Include(l => l.Resources)
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public IQueryable<Lesson> GetLessonWithResourcesAsync(int lessonId)
        {
            return _context.Lessons
                .Include(l => l.Resources)
                .Where(l => l.Id == lessonId);
        }
    }
}