using System;

namespace Etalem.Models.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Etalem.Models.Course Course { get; set; }

        public string CourseTitle { get; set; } 

        public string StudentId { get; set; }

        public string StudentName { get; set; } 

        public DateTime EnrollmentDate { get; set; }

        public bool IsCompleted { get; set; }

        public int Progress { get; set; }
        public string? CertificateUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}