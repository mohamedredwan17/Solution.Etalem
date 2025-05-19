namespace Etalem.Models
{
    public class CourseCategory
    {
       
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;
    }
}
