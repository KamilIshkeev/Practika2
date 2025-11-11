using System;

namespace Practika2.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }
        
        public string Content { get; set; } = string.Empty;
        
        public string? FilePath { get; set; }
        
        public int? Points { get; set; }
        
        public string? Comment { get; set; }
        
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? GradedAt { get; set; }
        
        public int AssignmentId { get; set; }
        
        public int StudentId { get; set; }
        
        // Navigation properties
        public Assignment Assignment { get; set; } = null!;
        public User Student { get; set; } = null!;
    }
}


