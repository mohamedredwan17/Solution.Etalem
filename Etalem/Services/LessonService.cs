using AutoMapper;
using Etalem.Data.Repo.Interfaces;
using Etalem.Infrastructure.Services;
using Etalem.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Etalem.Models.DTOs.Course;
using Etalem.Data;
using Microsoft.EntityFrameworkCore;

namespace Etalem.Services
{
    public class LessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IDiscussionRepository _discussionRepository;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;
        private readonly ApplicationDbContext _context;

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IDiscussionRepository discussionRepository,
            IFileService fileService,
            IMapper mapper,
            ILogger<LessonService> logger,
            ApplicationDbContext context)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _discussionRepository = discussionRepository;
            _fileService = fileService;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public async Task<int> CreateAsync(LessonDto dto, string instructorId)
        {
            _logger.LogInformation("Creating lesson for course: {CourseId} by Instructor: {InstructorId}", dto.CourseId, instructorId);
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("Checking course existence for CourseId: {CourseId} using DbContext", dto.CourseId);
                    var course = await _context.Courses
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == dto.CourseId);
                    if (course == null)
                    {
                        _logger.LogError("Course {CourseId} not found in DbContext", dto.CourseId);
                        throw new Exception($"Course with ID {dto.CourseId} not found in DbContext.");
                    }
                    _logger.LogInformation("Course {CourseId} found. InstructorId: {InstructorId}", dto.CourseId, course.InstructorId);

                    if (course.InstructorId != instructorId)
                    {
                        _logger.LogWarning("Instructor {InstructorId} is not authorized to add lessons to Course {CourseId}", instructorId, dto.CourseId);
                        throw new Exception("You are not authorized to add lessons to this course.");
                    }

                    _logger.LogInformation("Checking for existing lesson with Order: {Order} in Course: {CourseId}", dto.Order, dto.CourseId);
                    var existingLesson = await _lessonRepository.FindAsync(l => l.CourseId == dto.CourseId && l.Order == dto.Order);
                    if (existingLesson.Any())
                    {
                        _logger.LogWarning("Order {Order} already exists for course: {CourseId}", dto.Order, dto.CourseId);
                        throw new Exception("Lesson order already exists in this course.");
                    }

                    var lesson = _mapper.Map<Lesson>(dto);
                    lesson.CreatedAt = DateTime.UtcNow;

                    if (dto.Resources != null && dto.Resources.Any(r => r != null && r.ResourceFile != null))
                    {
                        _logger.LogInformation("Processing {Count} resources for lesson", dto.Resources.Count(r => r != null && r.ResourceFile != null));
                        lesson.Resources = new List<LessonResource>();
                        foreach (var resourceDto in dto.Resources.Where(r => r != null && r.ResourceFile != null))
                        {
                            if (resourceDto.ResourceFile.Length == 0)
                            {
                                _logger.LogWarning("Empty file provided for resource: {Title}", resourceDto.Title);
                                throw new Exception($"Resource file for '{resourceDto.Title}' cannot be empty.");
                            }

                            var fileUrl = await _fileService.UploadFileAsync(resourceDto.ResourceFile, "Lessons");
                            _logger.LogInformation("Uploaded file for resource '{Title}' to URL: {Url}", resourceDto.Title, fileUrl);

                            var lessonResource = new LessonResource
                            {
                                Title = resourceDto.Title,
                                ResourceUrl = fileUrl,
                                ResourceType = resourceDto.ResourceType,
                                LessonId = lesson.Id,
                            };
                            lesson.Resources.Add(lessonResource);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No resources provided for lesson");
                    }

                    var courseCount = await _context.Courses
                        .AsNoTracking()
                        .CountAsync(c => c.Id == dto.CourseId);
                    _logger.LogInformation("Manual DB check: Found {Count} courses with CourseId: {CourseId}", courseCount, dto.CourseId);
                    if (courseCount == 0)
                    {
                        _logger.LogError("Manual DB check failed: Course {CourseId} not found", dto.CourseId);
                        throw new Exception($"Manual DB check failed: Course with ID {dto.CourseId} not found.");
                    }

                    _logger.LogInformation("Saving lesson to database with CourseId: {CourseId}", lesson.CourseId);
                    await _lessonRepository.AddAsync(lesson);

                    if (lesson.Resources != null && lesson.Resources.Any())
                    {
                        foreach (var resource in lesson.Resources)
                        {
                            resource.LessonId = lesson.Id;
                            _logger.LogInformation("Assigned LessonId {LessonId} to resource: {ResourceTitle}", lesson.Id, resource.Title);
                        }
                        await _lessonRepository.UpdateAsync(lesson);
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed successfully for lesson creation");

                    _logger.LogInformation("Lesson created successfully with ID: {LessonId}, Duration: {Duration}, Resources Count: {ResourcesCount}",
                        lesson.Id, lesson.Duration, lesson.Resources?.Count ?? 0);
                    return lesson.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in transaction, rolling back for course: {CourseId}", dto.CourseId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson for course: {CourseId}", dto.CourseId);
                throw;
            }
        }

        public async Task<Course> GetCourseByIdAsync(int courseId)
        {
            _logger.LogInformation("Checking course existence for CourseId: {CourseId}", courseId);
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {CourseId} not found", courseId);
            }
            else
            {
                _logger.LogInformation("Course with ID {CourseId} found", courseId);
            }
            return course;
        }

        public async Task<LessonDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving lesson with ID: {LessonId}", id);
            var lesson = await _lessonRepository.GetLessonWithResourcesAsync(id)
                .Include(l => l.Discussions)
                .ThenInclude(d => d.User)
                .Include(l => l.Discussions)
                .ThenInclude(d => d.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync();
            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found: {LessonId}", id);
                throw new Exception("Lesson not found");
            }
            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByCourseAsync(int courseId)
        {
            _logger.LogInformation("Retrieving lessons for course: {CourseId}", courseId);
            var lessons = await _lessonRepository.GetLessonsByCourseAsync(courseId);
            return _mapper.Map<IEnumerable<LessonDto>>(lessons);
        }

        public async Task UpdateAsync(LessonDto dto)
        {
            _logger.LogInformation("Updating lesson with ID: {LessonId}", dto.Id);
            var lesson = await _lessonRepository.GetByIdAsync(dto.Id);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found: {LessonId}", dto.Id);
                throw new Exception("Lesson not found");
            }

            var existingLesson = await _lessonRepository.FindAsync(l => l.CourseId == dto.CourseId && l.Order == dto.Order && l.Id != dto.Id);
            if (existingLesson.Any())
            {
                _logger.LogWarning("Order {Order} already exists for course: {CourseId}", dto.Order, dto.CourseId);
                throw new Exception("Lesson order already exists in this course.");
            }

            var oldResources = lesson.Resources?.ToList() ?? new List<LessonResource>();
            _mapper.Map(dto, lesson);
            lesson.UpdatedAt = DateTime.UtcNow;

            if (dto.Resources != null && dto.Resources.Any(r => r.ResourceFile != null))
            {
                lesson.Resources = lesson.Resources ?? new List<LessonResource>();
                foreach (var resourceDto in dto.Resources.Where(r => r.ResourceFile != null))
                {
                    var fileUrl = await _fileService.UploadFileAsync(resourceDto.ResourceFile, "Lessons");
                    lesson.Resources.Add(new LessonResource
                    {
                        Title = resourceDto.Title,
                        ResourceUrl = fileUrl,
                        ResourceType = resourceDto.ResourceType,
                        LessonId = lesson.Id,
                    });
                }
            }

            foreach (var oldResource in oldResources.ToList())
            {
                if (dto.Resources == null || !dto.Resources.Any(r => r.ResourceUrl == oldResource.ResourceUrl && r.ResourceFile == null))
                {
                    await _fileService.DeleteFileAsync(oldResource.ResourceUrl);
                    lesson.Resources.Remove(oldResource);
                }
            }

            await _lessonRepository.UpdateAsync(lesson);
            _logger.LogInformation("Lesson updated successfully: {LessonId}, Duration: {Duration}, Resources Count: {ResourcesCount}",
                lesson.Id, lesson.Duration, lesson.Resources?.Count ?? 0);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting lesson with ID: {LessonId}", id);
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found: {LessonId}", id);
                throw new Exception("Lesson not found");
            }

            if (lesson.Resources != null)
            {
                foreach (var resource in lesson.Resources)
                {
                    if (!string.IsNullOrEmpty(resource.ResourceUrl))
                    {
                        await _fileService.DeleteFileAsync(resource.ResourceUrl);
                    }
                }
            }

            await _lessonRepository.DeleteAsync(lesson);
            _logger.LogInformation("Lesson deleted successfully: {LessonId}", id);
        }

        public async Task AddDiscussionAsync(Discussion discussion)
        {
            _logger.LogInformation("Adding discussion to lesson with ID: {LessonId}", discussion.LessonId);
            await _discussionRepository.AddAsync(discussion);
            _logger.LogInformation("Discussion added successfully to lesson: {LessonId}", discussion.LessonId);
        }
    }
}