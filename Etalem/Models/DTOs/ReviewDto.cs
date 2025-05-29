using System;
using Etalem.Models.DTOs.Course;
using Microsoft.AspNetCore.Identity;

namespace Etalem.Models.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        
        public string UserName { get; set; } 
        public int CourseId { get; set; }
        
        public string CourseTitle { get; set; } 
   
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}