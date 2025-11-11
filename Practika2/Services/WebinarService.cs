using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
	public class WebinarService
	{
		private readonly EduTrackContext _context;
		
		public WebinarService(EduTrackContext context)
		{
			_context = context;
		}
		
		public Task<List<WebinarSession>> GetUpcomingAsync(int lessonId)
		{
			var now = DateTime.UtcNow;
			return _context.WebinarSessions
				.Where(w => w.LessonId == lessonId && w.ScheduledAt >= now)
				.OrderBy(w => w.ScheduledAt)
				.ToListAsync();
		}
		
		public async Task<WebinarSession> ScheduleAsync(int lessonId, DateTime scheduledAtUtc, string link)
		{
			var s = new WebinarSession { LessonId = lessonId, ScheduledAt = scheduledAtUtc, Link = link };
			_context.WebinarSessions.Add(s);
			await _context.SaveChangesAsync();
			return s;
		}
	}
}





