using Etalem.Models;
using Etalem.Models.DTOs;
using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etalem.Areas.Identity.Pages.Instructor.Courses
{
    public class CoursesModel : PageModel
    {
        private readonly CourseService _courseService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CoursesModel> _logger;

        public CoursesModel(CourseService courseService, UserManager<IdentityUser> userManager, ILogger<CoursesModel> logger)
        {
            _courseService = courseService;
            _userManager = userManager;
            _logger = logger;
        }

        public IList<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalRevenues { get; set; }
        public double TotalAverageRating { get; set; }
        public int TotalLessons { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, double> RatingDistribution { get; set; } = new Dictionary<int, double>();
        public List<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>();

        public async Task OnGetAsync()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated");
                    TempData["ErrorMessage"] = "You must be logged in to view your courses.";
                    return;
                }

                Courses = (await _courseService.GetCoursesByInstructorAsync(userId)).ToList();

                TotalCourses = Courses.Count;
                TotalStudents = Courses.Sum(c => c.EnrollmentCount);
                TotalRevenues = Courses.Sum(c => c.Price * c.EnrollmentCount);
                TotalAverageRating = Courses.Any(c => c.AverageRating > 0) ? Courses.Where(c => c.AverageRating > 0).Average(c => c.AverageRating) : 0;
                TotalLessons = Courses.Sum(c => c.Lessons?.Count ?? 0);
                TotalReviews = Courses.Sum(c => c.Reviews?.Count ?? 0);

                
                var allReviews = Courses.SelectMany(c => c.Reviews ?? new List<ReviewDto>());
                var totalReviewsCount = allReviews.Count();
                if (totalReviewsCount > 0)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        RatingDistribution[i] = (allReviews.Count(r => r.Rating == i) * 100.0) / totalReviewsCount;
                    }
                }

                RecentReviews = Courses
                    .SelectMany(c => c.Reviews ?? new List<ReviewDto>())
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToList();

                if (!RecentReviews.Any())
                {
                    _logger.LogWarning("No recent reviews found for instructor {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses for instructor");
                TempData["ErrorMessage"] = "An error occurred while retrieving your courses.";
            }
        }
    }
}