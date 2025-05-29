using System;

namespace Etalem.Models.DTOs.Course
{
    public class AnswerDto
    {
        public int Id { get; set; }
        public int QuizAttemptId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int PointsEarned { get; set; }
        public string CorrectAnswer { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}