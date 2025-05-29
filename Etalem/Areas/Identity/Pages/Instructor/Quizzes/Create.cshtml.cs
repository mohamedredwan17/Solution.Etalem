using Etalem.Data;
using Etalem.Models;
using Etalem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Quizzes
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly QuizService _quizService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, QuizService quizService, ILogger<CreateModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _quizService = quizService;
            _logger = logger;
        }

        [BindProperty]
        public Quiz Quiz { get; set; } = new Quiz();

        [TempData]
        public string ErrorMessage { get; set; }

        public string CourseTitle { get; set; }

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            _logger.LogInformation("Loading Create Quiz page for course ID: {CourseId}", courseId);
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found for ID: {CourseId}", courseId);
                ErrorMessage = "Course not found.";
                return RedirectToPage("../Courses/Courses");
            }

            var userId = _userManager.GetUserId(User);
            if (course.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to manage course {CourseId}", userId, courseId);
                ErrorMessage = "You are not authorized to manage this course.";
                return RedirectToPage("../Courses/Courses");
            }

            CourseTitle = course.Title;
            Quiz.CourseId = courseId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int courseId)
        {
            _logger.LogInformation("Received POST request to create quiz for course ID: {CourseId}", courseId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                var course = await _context.Courses.FindAsync(courseId);
                CourseTitle = course?.Title ?? "Unknown Course";
                return Page();
            }

            var existingCourse = await _context.Courses.FindAsync(courseId);
            if (existingCourse == null)
            {
                _logger.LogWarning("Course not found for ID: {CourseId}", courseId);
                ErrorMessage = "Course not found.";
                return RedirectToPage("Index", new { courseId });
            }

            var userId = _userManager.GetUserId(User);
            if (existingCourse.InstructorId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to manage course {CourseId}", userId, courseId);
                ErrorMessage = "You are not authorized to manage this course.";
                return RedirectToPage("Index", new  {courseId});
            }

            try
            {
                Quiz.CreatedAt = DateTime.UtcNow;
                var createdQuiz = await _quizService.CreateQuizAsync(Quiz);
                _logger.LogInformation("Quiz created successfully with ID: {QuizId} for course ID: {CourseId}", createdQuiz.Id, courseId);
                return RedirectToPage("Index", new { courseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating quiz for course ID: {CourseId}", courseId);
                ErrorMessage = "An error occurred while creating the quiz. Please try again.";
                CourseTitle = existingCourse.Title;
                return Page();
            }
        }
    }
}