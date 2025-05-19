using Etalem.Models.DTOs.Course;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Etalem.Models;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    public class EditModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;

        public EditModel(ICourseService courseService, ICategoryService categoryService)
        {
            _courseService = courseService;
            _categoryService = categoryService;
        }

        [BindProperty] public CourseUpdateDto Course { get; set; } = new();
        [BindProperty] public List<int> SelectedCategoryIds { get; set; } = new();

        public List<Category> AllCategories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course is null) return NotFound();

            Course = new CourseUpdateDto
            {
                Id = course.Id,
                Title = course.Title,
                ShortDescription = course.ShortDescription,
                Price = course.Price
            };

            // Fix: Adjusted to handle the fact that course.Categories is a list of integers, not objects with an Id property.
            SelectedCategoryIds = course.Categories.ToList();
            AllCategories = (await _categoryService.GetAllCategoriesAsync()).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _courseService.UpdateCourseAsync(Course.Id, Course, SelectedCategoryIds, userId);
            return RedirectToPage("Index");
        }
    }
}
