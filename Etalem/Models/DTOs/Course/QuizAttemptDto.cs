using System;
using System.Collections.Generic;
using Etalem.Models.DTOs.Course;

namespace Etalem.Models.DTOs.Course
{
    public class QuizAttemptDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int QuizId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int Score { get; set; }
        public bool IsPassed { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }
}