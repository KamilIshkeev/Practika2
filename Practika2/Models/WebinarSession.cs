using System;

namespace Practika2.Models
{
	public class WebinarSession
	{
		public int Id { get; set; }
		
		public int LessonId { get; set; }
		public Lesson Lesson { get; set; } = null!;
		
		public DateTime ScheduledAt { get; set; }
		
		public string Link { get; set; } = string.Empty;
		
		public string? RecordingUrl { get; set; }
	}
}





