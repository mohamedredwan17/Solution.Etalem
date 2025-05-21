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

        public async Task<Course> GetCourseWithDetailsAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructorId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
        }
    }
}
