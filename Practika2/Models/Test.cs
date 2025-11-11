using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
	public class Test
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(200)]
		public string Title { get; set; } = string.Empty;
		
		public string Description { get; set; } = string.Empty;
		
		// Временной лимит в минутах (0 = без лимита)
		public int TimeLimitMinutes { get; set; }
		
		// Проходной балл в процентах (0-100)
		public int PassingScorePercent { get; set; } = 60;
		
		// Привязка к уроку
		public int LessonId { get; set; }
		public Lesson Lesson { get; set; } = null!;
		
		// Навигация
		public ICollection<TestQuestion> Questions { get; set; } = new List<TestQuestion>();
		public ICollection<TestSubmission> Submissions { get; set; } = new List<TestSubmission>();
	}
}





