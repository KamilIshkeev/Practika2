using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public int MaxPoints { get; set; }
        
        public DateTime? Deadline { get; set; }
        
        public int LessonId { get; set; }
        
        // Navigation properties
        public Lesson Lesson { get; set; } = null!;
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}


