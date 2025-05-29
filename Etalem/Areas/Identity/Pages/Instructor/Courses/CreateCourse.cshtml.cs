
using Etalem.Data;
using Etalem.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    [Authorize(Roles = "Instructor")]
    public class CreateCourseModel : PageModel
    {
        private readonly CourseService _courseService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateCourseModel> _logger;

        public CreateCourseModel(
            CourseService courseService,
            ApplicationDbContext context,
            ILogger<CreateCourseModel> logger)
        {
            _courseService = courseService;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public CourseDto Course { get; set; }

        public SelectList Categories { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Loading Create Course page");

            var categories = await _context.Categories.ToListAsync();
            Categories = new SelectList(categories, "Id", "Name");

            if (!categories.Any())
            {
                _logger.LogWarning("No categories found in the database");
                TempData["ErrorMessage"] = "No categories available. Please add a category first.";
                return RedirectToPage("/Instructor/Categories/Index");
            }

            Course = new CourseDto();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Received Course: Title={Title}, CategoryId={CategoryId}, Duration={Duration}",
                Course.Title, Course.CategoryId, Course.DurationInMinutes);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation Error: {ErrorMessage}", error.ErrorMessage);
                }

                // إعادة تحميل الكاتيجوريات في حالة وجود خطأ
                var categories = await _context.Categories.ToListAsync();
                Categories = new SelectList(categories, "Id", "Name");
                return Page();
            }

            try
            {
                var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(instructorId))
                {
                    _logger.LogError("Instructor ID not found in user claims");
                    ModelState.AddModelError(string.Empty, "Unable to identify the instructor.");
                    var categories = await _context.Categories.ToListAsync();
                    Categories = new SelectList(categories, "Id", "Name");
                    return Page();
                }

                await _courseService.CreateAsync(Course, instructorId);
                TempData["Message"] = "Course created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var categories = await _context.Categories.ToListAsync();
                Categories = new SelectList(categories, "Id", "Name");
                return Page();
            }
        }
    }
}