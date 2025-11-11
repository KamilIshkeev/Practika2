using System;

namespace Practika2.Models
{
	public class CourseCertificate
	{
		public int Id { get; set; }
		
		public int CourseId { get; set; }
		public Course Course { get; set; } = null!;
		
		public int StudentId { get; set; }
		public User Student { get; set; } = null!;
		
		public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
		
		public string? CertificateNumber { get; set; }
		public string? FilePath { get; set; }
	}
}





