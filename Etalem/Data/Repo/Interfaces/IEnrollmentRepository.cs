using Etalem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etalem.Data.Repo.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task AddAsync(Enrollment enrollment);
        Task<Enrollment> GetByIdAsync(int id);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(string studentId);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
        Task UpdateAsync(Enrollment enrollment);
        Task DeleteAsync(Enrollment enrollment);
    }
}