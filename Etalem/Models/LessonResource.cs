using System.ComponentModel.DataAnnotations;

namespace Etalem.Models
{
    public enum ResourceType
    {
        media,
        Docs
    }
    public class LessonResource
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lesson ID is required.")]
        public int LessonId { get; set; }

        public Lesson Lesson { get; set; }

        
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string? Title { get; set; }

        [Url(ErrorMessage = "Resource URL must be a valid URL.")]
        public string? ResourceUrl { get; set; } 

        [Required(ErrorMessage = "Resource Type is required.")]
        public ResourceType ResourceType { get; set; }

    }
}
