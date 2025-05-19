using Etalem.Models.DTOs.Course;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Etalem.Models;
using Microsoft.AspNetCore.Authorization;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    [Authorize(Roles = "Instructor")]
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;

        public CreateModel(ICourseService courseService, ICategoryService categoryService)
        {
            _courseService = courseService;
            _categoryService = categoryService;
        }

        [BindProperty] public CourseCreateDto Course { get; set; } = new();
        [BindProperty] public List<int> SelectedCategoryIds { get; set; } = new();
        [BindProperty] public List<Category> AllCategories { get; set; } = new();

        public async Task OnGetAsync()
        {
            AllCategories = (await _categoryService.GetAllCategoriesAsync()).ToList();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _courseService.CreateCourseAsync(Course, SelectedCategoryIds, userId);
            return RedirectToPage("Index");
        }
    }

}
