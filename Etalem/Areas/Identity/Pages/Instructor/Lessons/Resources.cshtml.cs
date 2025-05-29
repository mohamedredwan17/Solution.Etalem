using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Etalem.Areas.Identity.Pages.Instructor.Lessons
{
    [Authorize(Roles = "Instructor")]
    public class ResourcesModel : PageModel
    {
        private readonly LessonService _lessonService;
        private readonly LessonResourceService _lessonResourceService;

        public ResourcesModel(LessonService lessonService, LessonResourceService lessonResourceService)
        {
            _lessonService = lessonService;
            _lessonResourceService = lessonResourceService;
        }

        public LessonDto Lesson { get; set; }
        public int CourseId { get; set; }

        [BindProperty]
        public LessonResourceDto NewResource { get; set; }

        public async Task<IActionResult> OnGetAsync(int lessonId, int courseId)
        {
            var lesson = await _lessonService.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }
            Lesson = lesson;
            CourseId = courseId;
            NewResource = new LessonResourceDto();
            return Page();
        }

        public async Task<IActionResult> OnPostAddResourceAsync(int lessonId, int courseId)
        {
            if (!ModelState.IsValid)
            {
                var lesson = await _lessonService.GetByIdAsync(lessonId);
                Lesson = lesson;
                CourseId = courseId;
                return Page();
            }

            try
            {
                NewResource.LessonId = lessonId;
                await _lessonResourceService.CreateAsync(NewResource);
                TempData["Message"] = "Resource added successfully!";
                return RedirectToPage(new { lessonId, courseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var lesson = await _lessonService.GetByIdAsync(lessonId);
                Lesson = lesson;
                CourseId = courseId;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteResourceAsync(int resourceId, int lessonId, int courseId)
        {
            try
            {
                await _lessonResourceService.DeleteAsync(resourceId);
                TempData["Message"] = "Resource deleted successfully!";
                return RedirectToPage(new { lessonId, courseId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToPage(new { lessonId, courseId });
            }
        }
    }
}