using System;
using System.ComponentModel.DataAnnotations;

namespace Etalem.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        public int QuizAttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string SelectedAnswer { get; set; }

        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public QuizAttempt QuizAttempt { get; set; }
        public Question Question { get; set; }
    }
} 