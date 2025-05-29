using AutoMapper;
using Etalem.Data.Repo.Interfaces;
using Etalem.Infrastructure.Services;
using Etalem.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Etalem.Models.DTOs.Course;

namespace Etalem.Services
{
    public class LessonResourceService
    {
        private readonly ILessonResourceRepository _resourceRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonResourceService> _logger;

        public LessonResourceService(
            ILessonResourceRepository resourceRepository,
            ILessonRepository lessonRepository,
            IFileService fileService,
            IMapper mapper,
            ILogger<LessonResourceService> logger)
        {
            _resourceRepository = resourceRepository;
            _lessonRepository = lessonRepository;
            _fileService = fileService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<int> CreateAsync(LessonResourceDto dto)
        {
            _logger.LogInformation("Creating resource for lesson: {LessonId}", dto.LessonId);
            try
            {
                // التحقق إن الدرس موجود
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                if (lesson == null)
                {
                    _logger.LogWarning("Lesson not found: {LessonId}", dto.LessonId);
                    throw new Exception("Lesson not found.");
                }

                var resource = _mapper.Map<LessonResource>(dto);

                
                if (dto.ResourceFile != null)
                {
                    resource.ResourceUrl = await _fileService.UploadFileAsync(dto.ResourceFile, "Lessons");
                }
                else
                {
                    _logger.LogWarning("No file provided for resource for lesson: {LessonId}", dto.LessonId);
                    throw new Exception("A file is required to create a resource.");
                }

                await _resourceRepository.AddAsync(resource);
                _logger.LogInformation("Resource created successfully with ID: {ResourceId}", resource.Id);
                return resource.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resource for lesson: {LessonId}", dto.LessonId);
                throw;
            }
        }

        public async Task<LessonResourceDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving resource with ID: {ResourceId}", id);
            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                _logger.LogWarning("Resource not found: {ResourceId}", id);
                throw new Exception("Resource not found");
            }
            return _mapper.Map<LessonResourceDto>(resource);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Initiating deletion of resource with ID: {ResourceId}", id);
            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                _logger.LogWarning("Resource not found: {ResourceId}", id);
                throw new Exception("Resource not found");
            }

            if (!string.IsNullOrEmpty(resource.ResourceUrl))
            {
                _logger.LogInformation("Deleting file for resource: {ResourceUrl}", resource.ResourceUrl);
                await _fileService.DeleteFileAsync(resource.ResourceUrl);
            }
            _logger.LogInformation("Removing resource from repository: {ResourceId}", id);
            await _resourceRepository.DeleteAsync(resource);
            _logger.LogInformation("Resource deleted successfully: {ResourceId}", id);
        }
    }
}