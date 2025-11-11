using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
	public enum QuestionType
	{
		SingleChoice,
		MultipleChoice,
		TrueFalse
	}
	
	public class TestQuestion
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(500)]
		public string Text { get; set; } = string.Empty;
		
		public QuestionType Type { get; set; }
		
		public int Order { get; set; }
		
		public int TestId { get; set; }
		public Test Test { get; set; } = null!;
		
		public ICollection<TestAnswerOption> Options { get; set; } = new List<TestAnswerOption>();
	}
}





