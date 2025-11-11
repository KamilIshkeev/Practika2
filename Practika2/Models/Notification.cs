using System;

namespace Practika2.Models
{
	public enum NotificationType
	{
		Deadline,
		NewMaterial,
		GradingResult,
		System
	}
	
	public enum NotificationStatus
	{
		Pending,
		Sent,
		Read
	}
	
	public class Notification
	{
		public int Id { get; set; }
		
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		
		public NotificationType Type { get; set; }
		
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		
		public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
		
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		
		public DateTime? DueAt { get; set; } // для дедлайнов
		
		public DateTime? SentAt { get; set; }
		public DateTime? ReadAt { get; set; }
	}
}





