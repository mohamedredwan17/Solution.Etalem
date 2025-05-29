
using Etalem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Etalem.Models.DTOs.Course;
using Etalem.Services;

namespace Etalem.Areas.Identity.Pages.Instructor.Lessons
{
    public class CreateModel : PageModel
    {
        private readonly LessonService _lessonService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(LessonService lessonService, ILogger<CreateModel> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        [BindProperty]
        public LessonDto Lesson { get; set; }

        [BindProperty]
        public int CourseId { get; set; }

        public async Task<IActionResult> OnGet(int courseId)
        {
            _logger.LogInformation("Received request to create lesson for CourseId: {CourseId}", courseId);

            // التحقق من وجود الكورس قبل عرض الصفحة
            var courseExists = await _lessonService.GetCourseByIdAsync(courseId);
            if (courseExists == null)
            {
                _logger.LogWarning("Course with ID {CourseId} not found during OnGet", courseId);
                TempData["ErrorMessage"] = $"Course with ID {courseId} not found.";
                return RedirectToPage("/Instructor/Courses/Index");
            }

            CourseId = courseId;
            Lesson = new LessonDto { CourseId = courseId };
            _logger.LogInformation("Initialized LessonDto with CourseId: {CourseId}", Lesson.CourseId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Received Lesson: Title={Title}, CourseId={CourseId}, Resources Count={Count}",
                Lesson.Title, Lesson.CourseId, Lesson.Resources?.Count ?? 0);
            _logger.LogInformation("Received CourseId from BindProperty: {CourseId}", CourseId);

            
            if (Lesson.CourseId <= 0)
            {
                _logger.LogWarning("Lesson.CourseId is invalid, setting it from BindProperty CourseId: {CourseId}", CourseId);
                Lesson.CourseId = CourseId;
            }

            
            ModelState.Remove("Lesson.CourseId");

            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid after setting CourseId, checking errors...");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation Error: {ErrorMessage}", error.ErrorMessage);
                }
                return Page();
            }

            if (Lesson.Resources != null)
            {
                for (int i = 0; i < Lesson.Resources.Count; i++)
                {
                    _logger.LogInformation("Resource {Index}: Title={Title}, Type={Type}, File={FileName}, ResourceUrl={Url}",
                        i, Lesson.Resources[i].Title, Lesson.Resources[i].ResourceType,
                        Lesson.Resources[i].ResourceFile?.FileName ?? "None", Lesson.Resources[i].ResourceUrl);
                }
            }

            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    _logger.LogError("Instructor ID not found in user claims");
                    ModelState.AddModelError(string.Empty, "Unable to identify the instructor.");
                    return Page();
                }

                await _lessonService.CreateAsync(Lesson, instructorId);
                TempData["Message"] = "Lesson created successfully!";
                return RedirectToPage("Index", new { courseId = Lesson.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson for course: {CourseId}", Lesson.CourseId);
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return Page();
            }
        }
    }
}