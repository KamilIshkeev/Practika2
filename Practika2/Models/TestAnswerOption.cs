using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
	public class TestAnswerOption
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(300)]
		public string Text { get; set; } = string.Empty;
		
		// Флаг правильности
		public bool IsCorrect { get; set; }
		
		public int QuestionId { get; set; }
		public TestQuestion Question { get; set; } = null!;
	}
}





