using Etalem.Data.Repo.Interfaces;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;

namespace Etalem.Data.Repo
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        public CourseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }

        public async Task<IEnumerable<Course>> GetCoursesWithDetailsAsync()
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
