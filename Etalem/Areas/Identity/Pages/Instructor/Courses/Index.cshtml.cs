using Etalem.Models.DTOs.Course;
using Etalem.Models;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Etalem.Services;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    [Authorize(Roles = "Instructor")]
    public class CoursesModel : PageModel
    {
        private readonly CourseService _courseService;

        public CoursesModel(CourseService courseService)
        {
            _courseService = courseService;
        }

        public IEnumerable<CourseDto> Courses { get; set; }

        public async Task OnGetAsync()
        {
            var instructorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
        }
    }

}
