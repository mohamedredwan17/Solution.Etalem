using Etalem.Data;
using Etalem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Quizzes.Questions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public IList<Question> Questions { get; set; }
        public string QuizTitle { get; set; }
        public int QuizId { get; set; }
        public int? CourseId { get; set; } 

        public async Task<IActionResult> OnGetAsync(int quizId)
        {
            _logger.LogInformation("Loading Questions Index page for quiz ID: {QuizId}", quizId);
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found for ID: {QuizId}", quizId);
                TempData["ErrorMessage"] = "Quiz not found.";
                return RedirectToPage("../Index"); 
            }

            var userId = _userManager.GetUserId(User);
            if (quiz.Course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to manage quiz {QuizId}", userId, quizId);
                TempData["ErrorMessage"] = "You are not authorized to manage this quiz.";
                return RedirectToPage("../Index", new { courseId = quiz.CourseId });
            }

            QuizTitle = quiz.Title;
            QuizId = quizId;
            CourseId = quiz.CourseId; 
            Questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetDeleteAsync(int questionId, int quizId)
        {
            _logger.LogInformation("Received request to delete question ID: {QuestionId} for quiz ID: {QuizId}", questionId, quizId);

            var question = await _context.Questions
                .Include(q => q.Quiz)
                .ThenInclude(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.QuizId == quizId);

            if (question == null)
            {
                _logger.LogWarning("Question not found for ID: {QuestionId}", questionId);
                TempData["ErrorMessage"] = "Question not found.";
                return RedirectToPage(new { quizId });
            }

            var userId = _userManager.GetUserId(User);
            if (question.Quiz.Course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to delete question {QuestionId}", userId, questionId);
                TempData["ErrorMessage"] = "You are not authorized to delete this question.";
                return RedirectToPage(new { quizId });
            }

            try
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Question ID: {QuestionId} deleted successfully", questionId);
                TempData["SuccessMessage"] = "Question deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting question ID: {QuestionId}", questionId);
                TempData["ErrorMessage"] = "An error occurred while deleting the question. Please try again.";
            }

            return RedirectToPage(new { quizId });
        }
    }
}