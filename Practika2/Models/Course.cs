using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
    public enum CourseCategory
    {
        IT,
        Business,
        Design,
        Marketing,
        Languages,
        Other
    }

    public enum DifficultyLevel
    {
        Beginner,
        Advanced,
        Expert
    }

    public enum CourseFormat
    {
        Video,
        Text,
        Webinar,
        Mixed
    }

    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public CourseCategory Category { get; set; }
        
        public DifficultyLevel Difficulty { get; set; }
        
        public int Duration { get; set; } // in hours
        
        public decimal Price { get; set; }
        
        public bool IsFree => Price == 0;
        
        public CourseFormat Format { get; set; }
        
        public string? CoverImagePath { get; set; }
        
        public double Rating { get; set; }
        
        public int ReviewCount { get; set; }
        
        public bool IsPublished { get; set; }
        
        public bool IsArchived { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign keys
        public int CreatedById { get; set; }
        
        // Navigation properties
        public User CreatedBy { get; set; } = null!;
        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<CourseTeacher> Teachers { get; set; } = new List<CourseTeacher>();
    }
}


