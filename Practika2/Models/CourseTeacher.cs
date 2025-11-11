namespace Practika2.Models
{
    public class CourseTeacher
    {
        public int Id { get; set; }
        
        public int CourseId { get; set; }
        
        public int TeacherId { get; set; }
        
        // Navigation properties
        public Course Course { get; set; } = null!;
        public User Teacher { get; set; } = null!;
    }
}









