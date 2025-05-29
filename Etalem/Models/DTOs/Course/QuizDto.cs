using System;

namespace Etalem.Models.DTOs.Course
{
    public class QuizDto
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TimeLimit { get; set; } // in minutes
        public int PassingScore { get; set; } // percentage
        public int MaxAttempts { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<QuestionDto>? Questions { get; set; } = new List<QuestionDto>();
        public List<QuizAttemptDto>? QuizAttempts { get; set; } = new List<QuizAttemptDto>();
        
    }
}