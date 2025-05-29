using Etalem.Data;
using Etalem.Models;
using Etalem.Services;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Etalem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly CourseService _courseService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public CategoryController(ICategoryService categoryService, CourseService courseService, ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _categoryService = categoryService;
            _courseService = courseService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var courses = await _courseService.GetAllAsync();

            
            _logger.LogInformation("Retrieved {CategoryCount} categories", categories.Count());
            foreach (var category in categories)
            {
                _logger.LogInformation("Category ID: {CategoryId}, Name: {CategoryName}", category.Id, category.Name);
            }

           
            _logger.LogInformation("Retrieved {CourseCount} courses", courses.Count());
            foreach (var course in courses)
            {
                _logger.LogInformation("Course ID: {CourseId}, CategoryId: {CategoryId}, Title: {Title}", course.Id, course.CategoryId, course.Title);
            }

            // حساب عدد الكورسات لكل كاتيجوري
            var categoryCourseCounts = await _context.Courses
                .GroupBy(c => c.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            
            foreach (var entry in categoryCourseCounts)
            {
                _logger.LogInformation("Category ID: {CategoryId}, Course Count: {CourseCount}", entry.Key, entry.Value);
            }

            var categoryIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Web Development", "/images/icons/web-development.png" },
                { "Data Science", "/images/icons/data-science.png" },
                { "Marketing", "/images/icons/marketing.png" },
                { "IT & Software", "/images/icons/it-&-software.png" },
                { "Business", "/images/icons/business.png" },
                { "Photography", "/images/icons/photography.png" },
                { "UI UX Design", "/images/icons/ui-ux-design.png" },
                { "Mobile Application", "/images/icons/mobile-application.png" },
            };

            
            var courseDtos = courses.ToList();
            var instructorIds = courseDtos.Select(c => c.InstructorId).Distinct().ToList();
            var instructorEmails = await _context.Users
                .Where(u => instructorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            foreach (var course in courseDtos)
            {
                if (instructorEmails.ContainsKey(course.InstructorId))
                {
                    course.InstructorId = instructorEmails[course.InstructorId];
                }
            }

            ViewBag.Categories = categories;
            ViewBag.Courses = courseDtos;
            ViewBag.CategoryIcons = categoryIcons;
            ViewBag.CategoryCourseCounts = categoryCourseCounts;
            return View();
        }
    }
}