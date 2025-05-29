using Etalem.Data;
using Etalem.Models;
using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Etalem.Controllers
{
    public class CourseController : Controller
    {
        private readonly CourseService _courseService;
        private readonly ILogger<CourseController> _logger;
        private readonly ApplicationDbContext _context;

        public CourseController(CourseService courseService, ILogger<CourseController> logger, ApplicationDbContext context)
        {
            _courseService = courseService;
            _logger = logger;
            _context = context;
        }
        

        public async Task<IActionResult> Index(string categoryId, string level, string search)
        {
            try
            {
                var courses = await _courseService.GetAllAsync();

                // فلترة حسب الكاتيجوري
                if (!string.IsNullOrEmpty(categoryId))
                {
                    if (int.TryParse(categoryId, out int categoryIdInt))
                    {
                        courses = courses.Where(c => c.CategoryId == categoryIdInt).ToList();
                    }
                    else
                    {
                        _logger.LogWarning("Invalid categoryId format: {CategoryId}", categoryId);
                    }
                }

                // فلترة حسب المستوى
                if (!string.IsNullOrEmpty(level))
                {
                    if (Enum.TryParse<CourseLevel>(level, true, out var parsedLevel))
                    {
                        courses = courses.Where(c => c.Level == parsedLevel).ToList();
                    }
                    else
                    {
                        _logger.LogWarning("Invalid level format: {Level}", level);
                    }
                }

                // فلترة حسب السيرش
                if (!string.IsNullOrEmpty(search))
                {
                    courses = courses.Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                
                var categories = await _context.Categories.ToListAsync();
                
                var levels = Enum.GetValues(typeof(CourseLevel)).Cast<CourseLevel>().Select(l => l.ToString()).ToList();

                ViewBag.Categories = categories;
                ViewBag.Levels = levels;

                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                return View(new List<CourseDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetByIdAsync(id);

            if (course == null)
            {
                _logger.LogWarning("Course with ID {CourseId} not found", id);
                return NotFound();
            }

            return View(course);
        }
    }
}