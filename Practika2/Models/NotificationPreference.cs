namespace Practika2.Models
{
	public class NotificationPreference
	{
		public int Id { get; set; }
		
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		
		public bool DeadlinesEnabled { get; set; } = true;
		public bool NewMaterialsEnabled { get; set; } = true;
		public bool GradingResultsEnabled { get; set; } = true;
	}
}





