namespace Etalem.Models.DTOs.Course
{
    public class CourseCreateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string ShortDescription { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public CourseLevel Level { get; set; }
        public decimal? Price { get; set; }
        public List<int> CategoryIds { get; set; } = new();
    }
}
