using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Etalem.Models
{
    public class Discussion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public int? ParentDiscussionId { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

       
        public Lesson Lesson { get; set; }
        public IdentityUser User { get; set; }
        public Discussion ParentDiscussion { get; set; } // التعليق الأب
        public ICollection<Discussion> Replies { get; set; } = new List<Discussion>(); // الردود
    }
}