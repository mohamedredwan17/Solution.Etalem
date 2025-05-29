using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;
using Etalem.Models.DTOs.Course;
using Etalem.Services;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses;

[Authorize(Roles = "Instructor")]
public class DeleteCourseModel : PageModel
{
    private readonly CourseService _courseService;

    public DeleteCourseModel(CourseService courseService)
    {
        _courseService = courseService;
    }

    [BindProperty]
    public CourseDto Course { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Course = await _courseService.GetByIdAsync(id);

        if (Course == null || (await _courseService.GetCoursesByInstructorAsync(instructorId)).All(c => c.Id != id))
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if ((await _courseService.GetCoursesByInstructorAsync(instructorId)).All(c => c.Id != Course.Id))
        {
            return Forbid();
        }

        await _courseService.DeleteAsync(Course.Id);
        TempData["Message"] = "Course deleted successfully!";
        return RedirectToPage("Index" );
    }
}