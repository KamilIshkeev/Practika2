using System;

namespace Practika2.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public int ProgressPercentage { get; set; } // 0-100
        
        public int LessonId { get; set; }
        
        public int EnrollmentId { get; set; }
        
        // Navigation properties
        public Lesson Lesson { get; set; } = null!;
        public CourseEnrollment Enrollment { get; set; } = null!;
    }
}


