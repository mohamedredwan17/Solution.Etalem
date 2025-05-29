using Etalem.Data;
using Etalem.Models.DTOs.Course;
using Etalem.Models;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Etalem.Services;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    [Authorize(Roles = "Instructor")]
    public class EditCourseModel : PageModel
    {
        private readonly CourseService _courseService;
        private readonly ApplicationDbContext _context;

        public EditCourseModel(CourseService courseService, ApplicationDbContext context)
        {
            _courseService = courseService;
            _context = context;
        }

        [BindProperty]
        public CourseDto Course { get; set; }

        public SelectList Categories { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Course = await _courseService.GetByIdAsync(id);

            if (Course == null || (await _courseService.GetCoursesByInstructorAsync(instructorId)).All(c => c.Id != id))
            {
                return NotFound();
            }

            Categories = new SelectList(_context.Categories, "Id", "Name", Course.CategoryId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            //ModelState.Remove("ThumbnailUrl");
            
            //if (Course.ThumbnailFile == null)
            //{
            //    ModelState.Remove("ThumbnailFile");
            //}
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(_context.Categories, "Id", "Name", Course.CategoryId);
                return Page();
            }

            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if ((await _courseService.GetCoursesByInstructorAsync(instructorId)).All(c => c.Id != Course.Id))
            {
                return Forbid();
            }

            await _courseService.UpdateAsync(Course);
            TempData["Message"] = "Course updated successfully!";
            return RedirectToPage("Index");
        }
    }
}
