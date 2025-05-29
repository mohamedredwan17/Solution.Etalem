using System;
using System.Collections.Generic;

namespace Etalem.Models.DTOs.Course
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Text { get; set; } 
        public string CorrectAnswer { get; set; }
        public string Options { get; set; } 
        public int Points { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<AnswerDto>? Answers { get; set; } = new List<AnswerDto>();
    }
}