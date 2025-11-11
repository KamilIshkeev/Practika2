using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Practika2.Models
{
	public class Badge
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(100)]
		public string Code { get; set; } = string.Empty; // уникальный код
		
		[Required]
		[StringLength(200)]
		public string Title { get; set; } = string.Empty;
		
		public string Description { get; set; } = string.Empty;
		
		public string? Icon { get; set; } // путь к иконке
		
		public ICollection<UserBadge> AwardedTo { get; set; } = new List<UserBadge>();
	}
	
	public class UserBadge
	{
		public int Id { get; set; }
		
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		
		public int BadgeId { get; set; }
		public Badge Badge { get; set; } = null!;
		
		public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
	}
}





