using System.ComponentModel.DataAnnotations;

namespace Etalem.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course ID is required.")]
        public int? CourseId { get; set; }

        public Course? Course { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [StringLength(5000, ErrorMessage = "Content cannot exceed 5000 characters.")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 minute.")]
        public int Duration { get; set; } 

        [Required(ErrorMessage = "Order is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order must be a positive number.")]
        public int Order { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<LessonResource> Resources { get; set; } = new List<LessonResource>();
        public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
    }
}
