using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Etalem.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        [Required]
        public string Options { get; set; } 

        public int Points { get; set; }
        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
} 