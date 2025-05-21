using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Etalem.Models.DTOs.Course
{
    public class CourseDto
    {
        public int Id { get; set; } // معرف الكورس (للتعديل أو الحذف)

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

        // خاصية لرفع ملف الصورة
        [DataType(DataType.Upload)]
        [Display(Name = "Course Thumbnail Must be JPG or PNG")]
        public IFormFile? ThumbnailFile { get; set; }

        // خاصية لتخزين مسار الصورة بعد الحفظ
        [StringLength(500, ErrorMessage = "Thumbnail URL cannot exceed 500 characters")]
        public string? ThumbnailUrl { get; set; }

        [Required(ErrorMessage = "Learning objectives are required")]
        [StringLength(2000, ErrorMessage = "Learning objectives cannot exceed 2000 characters")]
        public string LearningObjectives { get; set; }

        [Required(ErrorMessage = "Requirements are required")]
        [StringLength(2000, ErrorMessage = "Requirements cannot exceed 2000 characters")]
        public string Requirements { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 10000, ErrorMessage = "Duration must be between 1 and 10,000 minutes")]
        public int DurationInMinutes { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        public bool IsPublished { get; set; } // للسماح للمدرب بتحديد حالة النشر
    }
}
