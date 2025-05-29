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
        public string LearningObjectives { get; set; }
        public string Requirements { get; set; } // متطلبات الكورس
        public int DurationInMinutes => Lessons?.Sum(l => l.Duration) ?? 0;
        public int EnrollmentCount { get; set; } 
        public double AverageRating { get; set; } 

        
        public string InstructorId { get; set; }
        public IdentityUser  Instructor { get; set; }
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
