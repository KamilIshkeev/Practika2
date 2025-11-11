using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
	public class DiscussionThread
	{
		public int Id { get; set; }
		
		public int CourseId { get; set; }
		public Course Course { get; set; } = null!;
		
		[Required]
		[StringLength(200)]
		public string Title { get; set; } = string.Empty;
		
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		
		public int AuthorId { get; set; }
		public User Author { get; set; } = null!;
		
		public ICollection<DiscussionMessage> Messages { get; set; } = new List<DiscussionMessage>();
	}
	
	public class DiscussionMessage
	{
		public int Id { get; set; }
		
		public int ThreadId { get; set; }
		public DiscussionThread Thread { get; set; } = null!;
		
		public int AuthorId { get; set; }
		public User Author { get; set; } = null!;
		
		[Required]
		[StringLength(2000)]
		public string Content { get; set; } = string.Empty;
		
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}





