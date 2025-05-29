using Etalem.Data;
using Etalem.Data.Repo.Interfaces;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etalem.Data.Repo
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Enrollment enrollment)
        {
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<Enrollment> GetByIdAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(string studentId)
        {
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                .ToListAsync();
        }

        public async Task UpdateAsync(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Enrollment enrollment)
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
        }
    }
}