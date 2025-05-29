using AutoMapper;
using Etalem.Data;
using Etalem.Models;
using Etalem.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewService(ApplicationDbContext context, IMapper mapper, ILogger<ReviewService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReviewDto> CreateReviewAsync(int courseId, int rating, string comment)
        {
            _logger.LogInformation("Creating new review for course ID: {CourseId}", courseId);

            
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No user ID found in current user context for course: {CourseId}", courseId);
                throw new Exception("User ID is required.");
            }

            
            var course = await _context.Courses
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", courseId);
                throw new Exception("Course not found.");
            }

            
            if (course.Reviews.Any(r => r.UserId == userId))
            {
                _logger.LogWarning("User {UserId} has already reviewed course {CourseId}", userId, courseId);
                throw new Exception("You have already reviewed this course.");
            }

            
            var review = new Review
            {
                UserId = userId,
                CourseId = courseId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // تحديث AverageRating
            course.AverageRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review created successfully for course ID: {CourseId} by user: {UserId}", courseId, userId);
            return _mapper.Map<ReviewDto>(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByCourseIdAsync(int courseId)
        {
            _logger.LogInformation("Retrieving reviews for course ID: {CourseId}", courseId);

            var reviews = await _context.Reviews
                .Where(r => r.CourseId == courseId)
                .Include(r => r.User)
                .Include(r => r.Course)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task DeleteReviewAsync(int reviewId)
        {
            _logger.LogInformation("Deleting review with ID: {ReviewId}", reviewId);

            var review = await _context.Reviews
                .Include(r => r.Course)
                    .ThenInclude(c => c.Reviews)
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
            {
                _logger.LogWarning("Review not found: {ReviewId}", reviewId);
                throw new Exception("Review not found.");
            }

            
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (review.UserId != userId)
            {
                _logger.LogWarning("User {UserId} is not authorized to delete review {ReviewId}", userId, reviewId);
                throw new Exception("You are not authorized to delete this review.");
            }

            _context.Reviews.Remove(review);

            // تحديث AverageRating
            var course = review.Course;
            course.AverageRating = course.Reviews.Any(r => r.Id != reviewId) ? course.Reviews.Where(r => r.Id != reviewId).Average(r => r.Rating) : 0;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review deleted successfully: {ReviewId}", reviewId);
        }
    }
}