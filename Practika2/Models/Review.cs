using System;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int CourseId { get; set; }
        
        public int UserId { get; set; }
        
        // Navigation properties
        public Course Course { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}


