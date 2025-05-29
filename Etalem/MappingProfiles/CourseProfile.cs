using AutoMapper;
using Etalem.Models.DTOs.Course;
using Etalem.Models;
using Etalem.Models.DTOs;

namespace Etalem.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.CompletionRate,
                    opt => opt.MapFrom(src => src.Enrollments.Any() ? src.Enrollments.Average(e => e.Progress) : 0))
                .ForMember(dest => dest.ContentItems, opt => opt.Ignore())
                .ForMember(dest => dest.ThumbnailFile, opt => opt.Ignore()) 
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src => src.Lessons))
                .ReverseMap()
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    CourseId = r.CourseId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserName = r.UserName,
                    CourseTitle = r.CourseTitle
                }) : null));

           
            CreateMap<CourseDto, Course>()
                .ForMember(dest => dest.Instructor, opt => opt.Ignore()) 
                .ForMember(dest => dest.Category, opt => opt.Ignore()) 
                .ForMember(dest => dest.EnrollmentCount, opt => opt.Ignore()) 
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()) 
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Lessons, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore()); 

            // Lesson mappings
            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.Discussions, opt => opt.MapFrom(src => src.Discussions != null ? src.Discussions.Where(d => d.ParentDiscussionId == null).Select(d => new DiscussionDto
                {
                    Id = d.Id,
                    LessonId = d.LessonId,
                    UserId = d.UserId,
                    Content = d.Content,
                    ParentDiscussionId = d.ParentDiscussionId,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    UserName = d.User != null ? d.User.UserName : "Anonymous",
                    Replies = d.Replies != null ? d.Replies.Select(r => new DiscussionDto
                    {
                        Id = r.Id,
                        LessonId = r.LessonId,
                        UserId = r.UserId,
                        Content = r.Content,
                        ParentDiscussionId = r.ParentDiscussionId,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        UserName = r.User != null ? r.User.UserName : "Anonymous"
                    }).ToList() : new List<DiscussionDto>()
                }) : null)); 
            CreateMap<LessonDto, Lesson>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // LessonResource mappings
            CreateMap<LessonResource, LessonResourceDto>()
                .ForMember(dest => dest.ResourceFile, opt => opt.Ignore()) 
                .ReverseMap();
            CreateMap<LessonResourceDto, LessonResource>()
                .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.LessonId))
                .ForMember(dest => dest.ResourceUrl, opt => opt.MapFrom(src => src.ResourceUrl))
                .ForMember(dest => dest.Lesson, opt => opt.Ignore());

            // Enrollment mappings
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.UserName))
                .ForMember(dest => dest.CertificateUrl, opt => opt.MapFrom(src => src.CertificateUrl))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.Course.ThumbnailUrl)); 

            CreateMap<EnrollmentDto, Enrollment>()
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Student, opt => opt.Ignore());

            // Quiz Mapping
            CreateMap<Quiz, QuizDto>()
                 .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions ?? new List<Question>()));
            CreateMap<QuizDto, Quiz>();

            // Question Mapping
            CreateMap<Question, QuestionDto>()
                 .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options ?? "[]")); 
            CreateMap<QuestionDto, Question>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => Newtonsoft.Json.JsonConvert.SerializeObject(src.Options)));

            // Answer Mapping
            CreateMap<Answer, AnswerDto>()
                .ForMember(dest => dest.CorrectAnswer, opt => opt.Ignore());
            CreateMap<AnswerDto, Answer>();

            // QuizAttempt Mapping
            CreateMap<QuizAttempt, QuizAttemptDto>()
                .ForMember(dest => dest.Answers, opt => opt.Ignore());
            CreateMap<QuizAttemptDto, QuizAttempt>()
                .ForMember(dest => dest.Answers, opt => opt.Ignore()); 

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Anonymous")) 
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course != null ? src.Course.Title : null));

            // Discussion Mapping
            CreateMap<Discussion, DiscussionDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Anonymous"))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies != null ? src.Replies.Select(r => new DiscussionDto
                {
                    Id = r.Id,
                    LessonId = r.LessonId,
                    UserId = r.UserId,
                    Content = r.Content,
                    ParentDiscussionId = r.ParentDiscussionId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserName = r.User != null ? r.User.UserName : "Anonymous"
                }).ToList() : new List<DiscussionDto>()));

        }
    }
}
