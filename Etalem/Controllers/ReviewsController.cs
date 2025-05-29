using Etalem.Models.DTOs;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etalem.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index(int courseId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByCourseIdAsync(courseId);
                ViewData["CourseId"] = courseId;
                return View(reviews);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Course", new { id = courseId });
            }
        }

        public IActionResult Create(int courseId)
        {
            var reviewDto = new ReviewDto
            {
                CourseId = courseId
            };
            return View(reviewDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int courseId, int rating, string comment)
        {
            try
            {
                var reviewDto = await _reviewService.CreateReviewAsync(courseId, rating, comment);
                TempData["SuccessMessage"] = "Review added successfully!";
                return RedirectToAction("Index", "Enrollment"); 
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Enrollment"); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int reviewId, int courseId)
        {
            try
            {
                await _reviewService.DeleteReviewAsync(reviewId);
                TempData["SuccessMessage"] = "Review deleted successfully!";
                return RedirectToAction("Index", new { courseId = courseId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { courseId = courseId });
            }
        }
    }
}