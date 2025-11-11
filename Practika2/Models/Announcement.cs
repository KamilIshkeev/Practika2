using System;

namespace Practika2.Models
{
	public class Announcement
	{
		public int Id { get; set; }
		
		public int CourseId { get; set; }
		public Course Course { get; set; } = null!;
		
		public int AuthorId { get; set; }
		public User Author { get; set; } = null!;
		
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}





