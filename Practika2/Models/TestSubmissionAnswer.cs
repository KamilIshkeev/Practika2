namespace Practika2.Models
{
	public class TestSubmissionAnswer
	{
		public int Id { get; set; }
		
		public int SubmissionId { get; set; }
		public TestSubmission Submission { get; set; } = null!;
		
		public int QuestionId { get; set; }
		public TestQuestion Question { get; set; } = null!;
		
		// Выбранная опция (для SingleChoice/TrueFalse)
		public int? SelectedOptionId { get; set; }
		public TestAnswerOption? SelectedOption { get; set; }
		
		// Для MultipleChoice — хранить как CSV ids (просто и быстро)
		public string? SelectedOptionIdsCsv { get; set; }
	}
}





