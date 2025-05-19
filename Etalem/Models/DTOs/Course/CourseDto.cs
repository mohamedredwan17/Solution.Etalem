namespace Etalem.Models.DTOs.Course
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string ShortDescription { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public CourseLevel Level { get; set; }
        public decimal? Price { get; set; }
        public string InstructorName { get; set; } = default!;
        public string InstructorId { get; set; } = default!;
        public List<int> Categories { get; set; } = new();
    }
}
