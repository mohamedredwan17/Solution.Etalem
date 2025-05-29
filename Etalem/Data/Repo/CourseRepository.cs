using Etalem.Data.Repo.Interfaces;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;

namespace Etalem.Data.Repo
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseRepository> _logger;

        public CourseRepository(ApplicationDbContext context, ILogger<CourseRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task<Course> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching course with ID: {CourseId}", id);
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {CourseId} not found in database", id);
            }
            else
            {
                _logger.LogInformation("Found course with ID: {CourseId}, Title: {Title}", id, course.Title);
            }
            return course;
        }

        public override async Task<IEnumerable<Course>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all courses");
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Reviews).ThenInclude(r => r.User) 
                .Include(c => c.Enrollments)
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} courses", courses.Count);
            return courses;
        }

        public override async Task<IEnumerable<Course>> FindAsync(Expression<Func<Course, bool>> predicate)
        {
            _logger.LogInformation("Finding courses with predicate");
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
                .Include(c => c.Enrollments)
                .Where(predicate)
                .ToListAsync();
            _logger.LogInformation("Found {Count} courses matching predicate", courses.Count);
            return courses;
        }

        public IQueryable<Course> GetCourseWithDetailsAsync(int id)
        {
            _logger.LogInformation("Fetching course with details for ID: {CourseId}", id);
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Reviews).ThenInclude(r => r.User) 
                .Include(c => c.Enrollments)
                .Where(c => c.Id == id);

            return query;
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructorId)
        {
            _logger.LogInformation("Fetching courses for instructor: {InstructorId}", instructorId);
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Reviews).ThenInclude(r => r.User)
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} courses for instructor {InstructorId}", courses.Count, instructorId);
            return courses;
        }

        public async Task<int> GetLessonsCountAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .CountAsync();
        }

        public async Task<int> GetDurationInMinutesAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .SumAsync(l => l.Duration);
        }

        public async Task ExecuteUpdateAsync(string sql, object[] parameters)
        {
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}