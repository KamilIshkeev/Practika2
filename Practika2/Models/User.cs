using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
    public enum UserRole
    {
        Administrator,
        Teacher,
        Student
    }

    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public UserRole Role { get; set; }
        
        public bool IsBlocked { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(255)]
        public string? ProfileImagePath { get; set; }

        // Navigation properties
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}


