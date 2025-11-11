using System;
using System.Collections.Generic;

namespace Practika2.Models
{
    public class CourseEnrollment
    {
        public int Id { get; set; }
        
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        public double Progress { get; set; } // 0-100
        
        public int? LastViewedLessonId { get; set; } // Закладка - последний просмотренный урок
        
        public int CourseId { get; set; }
        
        public int StudentId { get; set; }
        
        // Navigation properties
        public Course Course { get; set; } = null!;
        public User Student { get; set; } = null!;
        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}


