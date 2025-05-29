using Etalem.Data;
using Etalem.Models;
using Etalem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Quizzes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly QuizService _quizService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, QuizService quizService, ILogger<IndexModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _quizService = quizService;
            _logger = logger;
        }

        public IList<Quiz> Quizzes { get; set; }
        public string CourseTitle { get; set; }
        public int CourseId { get; set; }

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            _logger.LogInformation("Loading Quizzes Index page for course ID: {CourseId}", courseId);
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found for ID: {CourseId}", courseId);
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToPage("../Courses/Courses");
            }

            var userId = _userManager.GetUserId(User);
            if (course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to manage course {CourseId}", userId, courseId);
                TempData["ErrorMessage"] = "You are not authorized to manage this course.";
                return RedirectToPage("../Courses/Courses");
            }

            CourseTitle = course.Title;
            CourseId = courseId;
            Quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetDeleteAsync(int quizId, int courseId)
        {
            _logger.LogInformation("Received request to delete quiz ID: {QuizId} for course ID: {CourseId}", quizId, courseId);

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.CourseId == courseId);

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found for ID: {QuizId}", quizId);
                TempData["ErrorMessage"] = "Quiz not found.";
                return RedirectToPage(new { courseId });
            }

            var userId = _userManager.GetUserId(User);
            if (quiz.Course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to delete quiz {QuizId}", userId, quizId);
                TempData["ErrorMessage"] = "You are not authorized to delete this quiz.";
                return RedirectToPage(new { courseId });
            }

            try
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Quiz ID: {QuizId} deleted successfully", quizId);
                TempData["SuccessMessage"] = "Quiz deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting quiz ID: {QuizId}", quizId);
                TempData["ErrorMessage"] = "An error occurred while deleting the quiz. Please try again.";
            }

            return RedirectToPage(new { courseId });
        }
    }
}