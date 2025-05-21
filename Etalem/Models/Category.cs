using System.ComponentModel.DataAnnotations;

namespace Etalem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public List<Course> Courses { get; set; } = new List<Course>();

    }
}
