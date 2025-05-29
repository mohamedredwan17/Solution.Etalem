using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Etalem.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public IdentityUser Student { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int Progress { get; set; } = 0;
        public string? CertificateUrl { get; set; }

        public DateTime? CompletedAt { get; set; }

        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public bool ShouldCascadeOnDelete => false;
    }
}