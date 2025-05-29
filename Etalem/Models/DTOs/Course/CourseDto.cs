using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Etalem;
using Microsoft.AspNetCore.Identity;
using System.Security.Principal;


namespace Etalem.Models.DTOs.Course
{
    public class CourseDto
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Short description is required")]
        [StringLength(500, ErrorMessage = "Short description cannot exceed 500 characters")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Course level is required")]
        public CourseLevel Level { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10,000")]
        public decimal Price { get; set; }

        
        [DataType(DataType.Upload)]
        [Display(Name = "Course Thumbnail Must be JPG or PNG")]
        public IFormFile? ThumbnailFile { get; set; }

        
        [StringLength(500, ErrorMessage = "Thumbnail URL cannot exceed 500 characters")]
        public string? ThumbnailUrl { get; set; }

        [Required(ErrorMessage = "Learning objectives are required")]
        [StringLength(2000, ErrorMessage = "Learning objectives cannot exceed 2000 characters")]
        public string LearningObjectives { get; set; }

        [Required(ErrorMessage = "Requirements are required")]
        [StringLength(2000, ErrorMessage = "Requirements cannot exceed 2000 characters")]
        public string Requirements { get; set; }

        public int DurationInMinutes { get; set; }
        public int EnrollmentCount { get; set; }
        public double AverageRating { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? InstructorId { get; set; }
        public IdentityUser? Instructor { get; set; }
        public string? InstructorName { get; set; }
        public double CompletionRate { get; set; }

        public bool IsPublished { get; set; } 
        public List<LessonDto> Lessons { get; set; } = new List<LessonDto>();
        public List<QuizDto> Quizzes { get; set; } = new List<QuizDto>();
        public List<CourseContentItem>? ContentItems { get; set; }
        public ICollection<ReviewDto>? Reviews { get; set; }
        public List<EnrollmentDto> Enrollments { get; set; } = new List<EnrollmentDto>();
    }

    public class LessonDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public CourseDto? Course { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Duration { get; set; }
        public int Order { get; set; }
        public List<LessonResourceDto> Resources { get; set; } = new List<LessonResourceDto>();
        public List<LessonResourceDto> NewResources { get; set; } = new List<LessonResourceDto>();
        public ICollection<DiscussionDto> Discussions { get; set; } = new List<DiscussionDto>(); 
    }
    public class LessonResourceDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string? Title { get; set; }
        public IFormFile? ResourceFile { get; set; } 
        public string? ResourceUrl { get; set; }
        public ResourceType ResourceType { get; set; }
    }
    public class DiscussionDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public int? ParentDiscussionId { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserName { get; set; }
        public ICollection<DiscussionDto> Replies { get; set; } = new List<DiscussionDto>(); 
    }
}
