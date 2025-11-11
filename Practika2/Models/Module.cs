using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
    public class Module
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public int Order { get; set; }
        
        public int CourseId { get; set; }
        
        // Navigation properties
        public Course Course { get; set; } = null!;
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}


