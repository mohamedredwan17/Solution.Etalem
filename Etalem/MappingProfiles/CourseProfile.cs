using AutoMapper;
using Etalem.Models.DTOs.Course;
using Etalem.Models;

namespace Etalem.MappingProfiles
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            // Entity => CourseDto (للعرض)
            CreateMap<Course, CourseDto>();
                

            // CreateDto => Entity (عند الإنشاء)
            CreateMap<CourseCreateDto, Course>()
                .ForMember(dest => dest.CourseCategories, opt => opt.Ignore()); // هتربطهم يدويًا في السيرفيس غالبًا

            // UpdateDto => Entity (عند التحديث)
            CreateMap<CourseUpdateDto, Course>()
                .ForMember(dest => dest.CourseCategories, opt => opt.Ignore()); // نفس السبب
        }
    }
}
