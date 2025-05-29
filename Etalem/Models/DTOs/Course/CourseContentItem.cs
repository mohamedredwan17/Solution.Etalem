namespace Etalem.Models.DTOs.Course
{
    public class CourseContentItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public string Type { get; set; } // "Lesson" or "Quiz"
        public int Duration { get; set; } // For Lesson
        public int TimeLimit { get; set; } // For Quiz
        public int PassingScore { get; set; } // For Quiz
        public int MaxAttempts { get; set; } // For Quiz
        public bool IsCompleted { get; set; } // For Lesson
        public bool IsPassed { get; set; } // For Quiz
        public List<LessonResourceDto> Resources { get; set; } // For Lesson
    }
}
