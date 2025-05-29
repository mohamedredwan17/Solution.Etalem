using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Etalem.Models
{
    public class CompletedLesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public IdentityUser Student { get; set; }

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}