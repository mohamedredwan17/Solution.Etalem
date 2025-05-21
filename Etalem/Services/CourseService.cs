using AutoMapper;
using Etalem.Data.Repo.Interfaces;
using Etalem.Infrastructure.Services;
using Etalem.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Etalem.Models.DTOs.Course;
using Etalem.Services.Interfaces;

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
            var course = _mapper.Map<Course>(dto);
            course.InstructorId = userId;
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = null;

            if (dto.ThumbnailFile != null)
            {
                course.ThumbnailUrl = await _fileService.UploadFileAsync(dto.ThumbnailFile, "Courses");
            }
            else
            {
                course.ThumbnailUrl = "/Uploads/Courses/DefaultThumbnail.jpg";
            }

            await _courseRepository.AddAsync(course);
            return course.Id;
        }

        public async Task<CourseDto> GetByIdAsync(int id)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id);
            if (course == null)
            {
                throw new Exception("Course not found");
            }
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<IEnumerable<CourseDto>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> FindAsync(Expression<Func<Course, bool>> predicate)
        {
            var courses = await _courseRepository.FindAsync(predicate);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(string instructorId)
        {
            var courses = await _courseRepository.GetCoursesByInstructorAsync(instructorId);
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
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
                // الاحتفاظ بـ ThumbnailUrl القديمة
                course.ThumbnailUrl = oldThumbnailUrl;
            }

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course updated successfully: {CourseId}", dto.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                throw new Exception("Course not found");
            }

            if (!string.IsNullOrEmpty(course.ThumbnailUrl) && course.ThumbnailUrl != "/Uploads/Courses/DefaultThumbnail.jpg")
            {
                await _fileService.DeleteFileAsync(course.ThumbnailUrl);
            }

            await _courseRepository.DeleteAsync(course);
        }
    }
}