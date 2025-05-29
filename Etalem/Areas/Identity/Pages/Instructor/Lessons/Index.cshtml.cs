using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Lessons
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : PageModel
    {
        private readonly LessonService _lessonService;

        public IndexModel(LessonService lessonService)
        {
            _lessonService = lessonService;
        }

        public IEnumerable<LessonDto> Lessons { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }

        public async Task<IActionResult> OnGetAsync(int courseId)
        {
            CourseId = courseId;
            Lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
            
            CourseTitle = $"Course {courseId}"; 
            return Page();
        }
    }
}