using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using Etalem.Infrastructure.Services;
using Etalem.Data;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Etalem.Models.DTOs.Course;
using Etalem.Services;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    [Authorize(Roles = "Instructor")]
    public class CreateCourseModel : PageModel
    {
        private readonly CourseService _courseService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateCourseModel> _logger;

        public CreateCourseModel(CourseService courseService, ApplicationDbContext context, ILogger<CreateCourseModel> logger)
        {
            _courseService = courseService;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public CourseDto Course { get; set; }

        public SelectList Categories { get; set; }

        public IActionResult OnGet()
        {
            Course = new CourseDto();
            Categories = new SelectList(_context.Categories, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("OnPostAsync called for CreateCourse");

            

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                Categories = new SelectList(_context.Categories, "Id", "Name");
                return Page();
            }

            if (Course.ThumbnailFile == null)
            {
                _logger.LogWarning("ThumbnailFile is null");
            }
            else
            {
                _logger.LogInformation("ThumbnailFile received: {FileName}, Size: {FileSize}", Course.ThumbnailFile.FileName, Course.ThumbnailFile.Length);
            }

            try
            {
                var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var courseId = await _courseService.CreateAsync(Course, instructorId);
                TempData["Message"] = "Course created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                ModelState.AddModelError("", "An error occurred while creating the course. Please try again.");
                Categories = new SelectList(_context.Categories, "Id", "Name");
                return Page();
            }
        }
    }
}