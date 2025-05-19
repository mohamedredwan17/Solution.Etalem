using AutoMapper;
using Etalem.Data.Repo.Interfaces;
using Etalem.Models.DTOs.Course;
using Etalem.Models;
using Etalem.Services.Interfaces;

namespace Etalem.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        public CourseService(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDto>> GetCoursesByInstructorIdAsync(string instructorId)
        {
            var courses = await _courseRepository.GetCoursesWithDetailsAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses.Where(c => c.InstructorId == instructorId));
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetCoursesWithDetailsAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id);
            return course is null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task CreateCourseAsync(CourseCreateDto courseDto, List<int> categoryIds, string instructorId)
        {
            var course = _mapper.Map<Course>(courseDto);
            course.InstructorId = instructorId;

            course.CourseCategories = categoryIds
                .Select(catId => new CourseCategory
                {
                    CategoryId = catId
                }).ToList();

            await _courseRepository.AddAsync(course);
        }


        public async Task UpdateCourseAsync(int id, CourseUpdateDto dto, List<int> categoryIds, string instructorId)
        {
            var existingCourse = await _courseRepository.GetCourseWithDetailsAsync(id);
            if (existingCourse == null)
                throw new Exception("Course not found");

            // التأكد أن المستخدم هو الإنستراكتور صاحب الكورس
            if (existingCourse.InstructorId != instructorId)
                throw new UnauthorizedAccessException("You are not authorized to update this course");

            // تحديث البيانات الأساسية
            _mapper.Map(dto, existingCourse);

            // تحديث روابط التصنيفات
            existingCourse.CourseCategories = categoryIds.Select(cid => new CourseCategory
            {
                CourseId = id,
                CategoryId = cid
            }).ToList();

            await _courseRepository.UpdateAsync(existingCourse);
        }



        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return false;

            _courseRepository.DeleteAsync(course);
            return true;
        }
    }
}
