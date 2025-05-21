using Microsoft.AspNetCore.Identity;

namespace Etalem.Models
{
    public enum CourseLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        AllLevels
    }

    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public CourseLevel Level { get; set; }
        public string? ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsFree => Price == 0;
        public bool IsPublished { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } 
        public string LearningObjectives { get; set; } // أهداف التعلم
        public string Requirements { get; set; } // متطلبات الكورس
        public int DurationInMinutes { get; set; } // مدة الكورس بالدقائق
        public int EnrollmentCount { get; set; } // عدد المسجلين (محسوب)
        public double AverageRating { get; set; } // متوسط التقييم (محسوب)

        // العلاقات
        public string InstructorId { get; set; }
        public IdentityUser  Instructor { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
