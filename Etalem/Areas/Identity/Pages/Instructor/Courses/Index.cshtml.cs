using Etalem.Models.DTOs.Course;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;
        public List<CourseDto> Courses { get; set; } = new();

        public IndexModel(ICourseService courseService)
        {
            _courseService = courseService;
        }


        public async Task OnGetAsync()
        {
            var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Courses = (await _courseService.GetCoursesByInstructorIdAsync(instructorId)).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            return RedirectToPage();
        }
    }

}
