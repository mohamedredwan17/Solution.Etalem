using AutoMapper;
using Etalem.Models.DTOs.Course;
using Etalem.Models;

namespace Etalem.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // التحويل من Course إلى CourseDto
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.ThumbnailFile, opt => opt.Ignore()); // نتجاهل ThumbnailFile لأنه مش موجود في Course

            // التحويل من CourseDto إلى Course
            CreateMap<CourseDto, Course>()
                .ForMember(dest => dest.Instructor, opt => opt.Ignore()) // نتجاهل Instructor لأنه يتم تعيينه يدويًا
                .ForMember(dest => dest.Category, opt => opt.Ignore()) // نتجاهل Category لأنه يتم تعيينه يدويًا
                .ForMember(dest => dest.EnrollmentCount, opt => opt.Ignore()) // محسوب، مش من DTO
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore()) // محسوب، مش من DTO
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // يتم تعيينه تلقائيًا
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) // يتم تعيينه في CourseService
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.Ignore()); // يتم تعيينه في CourseService
        }
    }
}
