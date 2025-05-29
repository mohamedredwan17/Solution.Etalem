using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Etalem.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        
        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public int TimeLimit { get; set; } // in minutes
        public int PassingScore { get; set; } // percentage
        public int MaxAttempts { get; set; }
        [Required]
        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        
        public ICollection<Question>? Questions { get; set; }
        public ICollection<QuizAttempt>? Attempts { get; set; }
    }
}