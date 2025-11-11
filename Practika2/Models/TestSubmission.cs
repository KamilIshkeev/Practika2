using System;
using System.Collections.Generic;

namespace Practika2.Models
{
	public class TestSubmission
	{
		public int Id { get; set; }
		
		public int TestId { get; set; }
		public Test Test { get; set; } = null!;
		
		public int StudentId { get; set; }
		public User Student { get; set; } = null!;
		
		// Итоговый процент
		public int ScorePercent { get; set; }
		
		public bool IsPassed { get; set; }
		
		public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
		
		public ICollection<TestSubmissionAnswer> Answers { get; set; } = new List<TestSubmissionAnswer>();
	}
}





