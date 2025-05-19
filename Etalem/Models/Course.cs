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

        public string Title { get; set; } = default!;
        public string ShortDescription { get; set; } = default!;
        public string? ImageUrl { get; set; }

        public CourseLevel Level { get; set; }

        // العلاقة مع الـ Instructor (ApplicationUser)
        public string InstructorId { get; set; } = default!;
        public IdentityUser Instructor { get; set; } = default!;

        public decimal? Price { get; set; }
        public double? Rating { get; set; }
        public int? StudentsCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<CourseCategory> CourseCategories { get; set; } = new List<CourseCategory>();
    }
}
