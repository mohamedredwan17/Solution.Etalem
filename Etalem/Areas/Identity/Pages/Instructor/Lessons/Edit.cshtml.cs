using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Lessons
{
    [Authorize(Roles = "Instructor")]
    public class EditModel : PageModel
    {
        private readonly LessonService _lessonService;
        private readonly LessonResourceService _lessonResourceService;

        public EditModel(LessonService lessonService, LessonResourceService lessonResourceService)
        {
            _lessonService = lessonService;
            _lessonResourceService = lessonResourceService;
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Get the existing lesson to preserve resources
                var existingLesson = await _lessonService.GetByIdAsync(Lesson.Id);
                if (existingLesson != null)
                {
                    // Preserve existing resources and merge with any new ones
                    var existingResources = existingLesson.Resources.ToList();
                    if (Lesson.Resources != null && Lesson.Resources.Any())
                    {
                        foreach (var newResource in Lesson.Resources)
                        {
                            if (newResource.Id == 0) // New resource
                            {
                                existingResources.Add(newResource);
                            }
                            else // Existing resource, update only if modified
                            {
                                var existingResource = existingResources.FirstOrDefault(r => r.Id == newResource.Id);
                                if (existingResource != null)
                                {
                                    existingResource.Title = newResource.Title;
                                    existingResource.ResourceType = newResource.ResourceType;
                                    // Keep ResourceUrl and other unchanged fields from existing
                                    newResource.ResourceUrl = existingResource.ResourceUrl;
                                }
                            }
                        }
                    }
                    Lesson.Resources = existingResources;
                }

                await _lessonService.UpdateAsync(Lesson);
                TempData["Message"] = "Lesson updated successfully!";
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