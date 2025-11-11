using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
	public class CertificateService
	{
		private readonly EduTrackContext _context;
		
		public CertificateService(EduTrackContext context)
		{
			_context = context;
		}
		
		public async Task<CourseCertificate?> IssueIfEligibleAsync(int courseId, int studentId)
		{
			// Проверка завершения
			var enrollment = await _context.CourseEnrollments
				.FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
			if (enrollment == null || enrollment.Progress < 100)
				return null;
			
			// Уже выдан?
			var existing = await _context.CourseCertificates
				.FirstOrDefaultAsync(c => c.CourseId == courseId && c.StudentId == studentId);
			if (existing != null) return existing;
			
			var cert = new CourseCertificate
			{
				CourseId = courseId,
				StudentId = studentId,
				IssuedAt = DateTime.UtcNow,
				CertificateNumber = $"EDU-{courseId}-{studentId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
			};
			_context.CourseCertificates.Add(cert);
			await _context.SaveChangesAsync();
			return cert;
		}
	}
}





