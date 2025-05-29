using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Lessons
{
    [Authorize(Roles = "Instructor")]
    public class DeleteModel : PageModel
    {
        private readonly LessonService _lessonService;

        public DeleteModel(LessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [BindProperty]
        public LessonDto Lesson { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, int courseId)
        {
            var lesson = await _lessonService.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            Lesson = lesson;
            Lesson.CourseId = courseId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _lessonService.DeleteAsync(Lesson.Id);
                TempData["Message"] = "Lesson deleted successfully!";
                return RedirectToPage("Index", new { courseId = Lesson.CourseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return Page();
            }
        }
    }
}