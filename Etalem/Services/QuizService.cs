using Etalem.Data;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class QuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizService> _logger;

        public QuizService(ApplicationDbContext context, ILogger<QuizService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Quiz> CreateQuizAsync(Quiz quiz)
        {
            _logger.LogInformation("Creating new quiz for course ID: {CourseId}", quiz.CourseId);
            quiz.CreatedAt = DateTime.UtcNow;
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task<Quiz> GetQuizByIdAsync(int quizId)
        {
            _logger.LogInformation("Retrieving quiz with ID: {QuizId}", quizId);
            return await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }
    }
}