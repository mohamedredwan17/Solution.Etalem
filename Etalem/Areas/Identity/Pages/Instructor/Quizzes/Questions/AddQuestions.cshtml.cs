using Etalem.Data;
using Etalem.Models;
using Etalem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace Etalem.Areas.Identity.Pages.Instructor.Quizzes.Questions
{
    public class AddQuestionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly QuestionService _questionService;
        private readonly ILogger<AddQuestionsModel> _logger; 

        public AddQuestionsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, QuestionService questionService, ILogger<AddQuestionsModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _questionService = questionService;
            _logger = logger;
        }

        [BindProperty]
        public Question Question { get; set; } = new Question();

        [TempData]
        public string ErrorMessage { get; set; }

        public string QuizTitle { get; set; }

        public async Task<IActionResult> OnGetAsync(int quizId)
        {
            _logger.LogInformation("Loading Add Questions page for quiz ID: {QuizId}", quizId);
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found for ID: {QuizId}", quizId);
                ErrorMessage = "Quiz not found.";
                return RedirectToPage("Index", new { quizId });
            }

            var userId = _userManager.GetUserId(User);
            if (quiz.Course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to manage quiz {QuizId}", userId, quizId);
                ErrorMessage = "You are not authorized to manage this quiz.";
                return RedirectToPage("Index", new { quizId });
            }

            QuizTitle = quiz.Title;
            Question.QuizId = quizId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int quizId)
        {
            _logger.LogInformation("Received POST request to add question for quiz ID: {QuizId}. Question: {Question}", quizId, JsonConvert.SerializeObject(Question));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                var quiz = await _context.Quizzes.FindAsync(quizId);
                QuizTitle = quiz?.Title ?? "Unknown Quiz";
                return Page();
            }

            var existingQuiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (existingQuiz == null)
            {
                _logger.LogWarning("Quiz not found for ID: {QuizId}", quizId);
                ErrorMessage = "Quiz not found.";
                return RedirectToPage("Index", new { quizId });
            }

            var userId = _userManager.GetUserId(User);
            if (existingQuiz.Course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to manage quiz {QuizId}", userId, quizId);
                ErrorMessage = "You are not authorized to manage this quiz.";
                return RedirectToPage("Index", new { quizId });
            }

            try
            {
                
                var optionsList = Question.Options.Split(',').Select(o => o.Trim()).ToList();
                Question.Options = JsonConvert.SerializeObject(optionsList);
                Question.CreatedAt = DateTime.UtcNow;
                Question.QuizId = quizId; 
                _logger.LogInformation("Adding question to database: {Question}", JsonConvert.SerializeObject(Question));
                await _questionService.CreateQuestionAsync(Question);
                _logger.LogInformation("Question added successfully for quiz ID: {QuizId}", quizId);
                return RedirectToPage("Index", new { quizId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding question for quiz ID: {QuizId}", quizId);
                ErrorMessage = "An error occurred while adding the question. Please try again.";
                QuizTitle = existingQuiz.Title;
                return Page();
            }
        }
    }
}