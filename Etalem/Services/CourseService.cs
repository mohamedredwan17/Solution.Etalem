using AutoMapper;
using Etalem.Data.Repo.Interfaces;
using Etalem.Infrastructure.Services;
using Etalem.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Etalem.Models.DTOs.Course;
using Etalem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Etalem.Services
{
    public class CourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(ICourseRepository courseRepository, IFileService fileService, IMapper mapper, ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _fileService = fileService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<int> CreateAsync(CourseDto dto, string userId)
        {
            _logger.LogInformation("Creating course for instructor: {UserId}", userId);
            try
            {
                var course = _mapper.Map<Course>(dto);
                course.InstructorId = userId;
                course.CreatedAt = DateTime.UtcNow;
                course.UpdatedAt = null;

                if (dto.ThumbnailFile != null)
                {
                    _logger.LogInformation("Processing ThumbnailFile for course");
                    course.ThumbnailUrl = await _fileService.UploadFileAsync(dto.ThumbnailFile, "Courses");
                }
                else
                {
                    _logger.LogWarning("No ThumbnailFile provided, using default thumbnail");
                    course.ThumbnailUrl = "/Uploads/Courses/DefaultThumbnail.jpg";
                }
                if (course.IsPublished && course.DurationInMinutes == 0)
                {
                    _logger.LogWarning("Cannot create a published course with no lessons for instructor: {UserId}", userId);
                    throw new Exception("Cannot create a published course with no lessons.");
                }

                await _courseRepository.AddAsync(course);
                _logger.LogInformation("Course created successfully with ID: {CourseId}, DurationInMinutes: {Duration}, Lessons Count: {LessonsCount}",
                    course.Id, course.DurationInMinutes, course.Lessons?.Count ?? 0);
                return course.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course for instructor: {UserId}", userId);
                throw;
            }
        }

        public async Task<CourseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving course with ID: {CourseId}", id);
            var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", id);
                throw new Exception("Course not found");
            }

            course.AverageRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0;
            course.EnrollmentCount = course.Enrollments.Count;

            var courseDto = _mapper.Map<CourseDto>(course);
            courseDto.InstructorName = course.Instructor?.UserName ?? "Unknown Instructor"; 

            if (course.Reviews != null && courseDto.Reviews == null)
            {
                courseDto.Reviews = course.Reviews.Select(r => _mapper.Map<ReviewDto>(r)).ToList();
            }

            _logger.LogInformation("Course retrieved with DurationInMinutes: {Duration}, Lessons Count: {LessonsCount}, AverageRating: {AverageRating}, EnrollmentCount: {EnrollmentCount}",
                courseDto.DurationInMinutes, course.Lessons?.Count ?? 0, courseDto.AverageRating, courseDto.EnrollmentCount);
            return courseDto;
        }

        public async Task<IEnumerable<CourseDto>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all courses");
            var courses = await _courseRepository.GetAllAsync();
            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
            foreach (var courseDto in courseDtos)
            {
                var course = courses.FirstOrDefault(c => c.Id == courseDto.Id);
                courseDto.InstructorName = course?.Instructor?.UserName ?? "Unknown Instructor"; 
            }
            _logger.LogInformation("Retrieved {Count} courses", courseDtos.Count());
            return courseDtos;
        }

        public async Task<IEnumerable<CourseDto>> FindAsync(Expression<Func<Course, bool>> predicate)
        {
            _logger.LogInformation("Finding courses with predicate");
            var courses = await _courseRepository.FindAsync(predicate);
            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
            foreach (var courseDto in courseDtos)
            {
                var course = courses.FirstOrDefault(c => c.Id == courseDto.Id);
                courseDto.InstructorName = course?.Instructor?.UserName ?? "Unknown Instructor"; 
            }
            return courseDtos;
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(string instructorId)
        {
            _logger.LogInformation("Retrieving courses for instructor: {InstructorId}", instructorId);
            var courses = await _courseRepository.GetCoursesByInstructorAsync(instructorId);
            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
            foreach (var courseDto in courseDtos)
            {
                var course = courses.FirstOrDefault(c => c.Id == courseDto.Id);
                courseDto.InstructorName = course?.Instructor?.UserName ?? "Unknown Instructor"; 
            }
            return courseDtos;
        }

        public async Task UpdateAsync(CourseDto dto)
        {
            _logger.LogInformation("Updating course with ID: {CourseId}", dto.Id);

            var course = await _courseRepository.GetByIdAsync(dto.Id);
            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", dto.Id);
                throw new Exception("Course not found");
            }

            var lessonsCount = await _courseRepository.GetLessonsCountAsync(dto.Id);
            var durationInMinutes = await _courseRepository.GetDurationInMinutesAsync(dto.Id);

            if (dto.IsPublished && durationInMinutes == 0)
            {
                _logger.LogWarning("Cannot publish course {CourseId} with no lessons", dto.Id);
                throw new Exception("Cannot publish a course with no lessons.");
            }

            var oldThumbnailUrl = course.ThumbnailUrl;
            _mapper.Map(dto, course);
            course.UpdatedAt = DateTime.UtcNow;

            if (dto.ThumbnailFile != null)
            {
                _logger.LogInformation("Processing new ThumbnailFile for course: {CourseId}", dto.Id);
                course.ThumbnailUrl = await _fileService.UploadFileAsync(dto.ThumbnailFile, "Courses");
                if (!string.IsNullOrEmpty(oldThumbnailUrl) && oldThumbnailUrl != "/Uploads/Courses/DefaultThumbnail.jpg")
                {
                    await _fileService.DeleteFileAsync(oldThumbnailUrl);
                }
            }
            else
            {
                _logger.LogInformation("No new ThumbnailFile provided, keeping existing ThumbnailUrl: {ThumbnailUrl}", course.ThumbnailUrl);
                course.ThumbnailUrl = oldThumbnailUrl;
            }

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course updated successfully: {CourseId}, DurationInMinutes: {Duration}, Lessons Count: {LessonsCount}",
                dto.Id, durationInMinutes, lessonsCount);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting course with ID: {CourseId}", id);
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", id);
                throw new Exception("Course not found");
            }

            if (!string.IsNullOrEmpty(course.ThumbnailUrl) && course.ThumbnailUrl != "/Uploads/Courses/DefaultThumbnail.jpg")
            {
                await _fileService.DeleteFileAsync(course.ThumbnailUrl);
            }

            await _courseRepository.DeleteAsync(course);
            _logger.LogInformation("Course deleted successfully: {CourseId}", id);
        }
    }
}