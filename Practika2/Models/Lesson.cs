using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
    public enum LessonType
    {
        Video,
        Text,
        PDF,
        Link,
        Webinar
    }

    public class Lesson
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public LessonType Type { get; set; }
        
        public string? ContentUrl { get; set; }
        
        public string? Content { get; set; } // For text content
        
        public int Duration { get; set; } // in minutes
        
        public int Order { get; set; }
        
        public int ModuleId { get; set; }
        
        // Navigation properties
        public Module Module { get; set; } = null!;
        public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}


