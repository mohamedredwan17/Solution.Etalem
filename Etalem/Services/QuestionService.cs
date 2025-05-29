using Etalem.Data;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class QuestionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuestionService> _logger;

        public QuestionService(ApplicationDbContext context, ILogger<QuestionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Question> CreateQuestionAsync(Question question)
        {
            _logger.LogInformation("Creating new question for quiz ID: {QuizId}", question.QuizId);
            question.CreatedAt = DateTime.UtcNow;
            question.Options = JsonConvert.SerializeObject(question.Options);
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<List<Question>> GetQuestionsByQuizIdAsync(int quizId)
        {
            _logger.LogInformation("Retrieving questions for quiz ID: {QuizId}", quizId);
            return await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();
        }
    }
}