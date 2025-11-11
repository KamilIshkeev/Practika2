using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;

namespace Practika2.Services
{
	public class GamificationService
	{
		private readonly EduTrackContext _context;
		
		public GamificationService(EduTrackContext context)
		{
			_context = context;
		}
		
		// Пример: дать бейдж за первую отправку задания
		public async Task AwardFirstSubmissionBadgeAsync(int studentId)
		{
			var hasSubmissions = await _context.AssignmentSubmissions.AnyAsync(s => s.StudentId == studentId);
			if (!hasSubmissions) return;
			
			var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Code == "FIRST_SUBMISSION");
			if (badge == null)
			{
				badge = new Models.Badge
				{
					Code = "FIRST_SUBMISSION",
					Title = "Первая отправка",
					Description = "Вы отправили первое домашнее задание"
				};
				_context.Badges.Add(badge);
				await _context.SaveChangesAsync();
			}
			
			var already = await _context.UserBadges.AnyAsync(ub => ub.UserId == studentId && ub.BadgeId == badge.Id);
			if (!already)
			{
				_context.UserBadges.Add(new Models.UserBadge { UserId = studentId, BadgeId = badge.Id, AwardedAt = DateTime.UtcNow });
				await _context.SaveChangesAsync();
			}
		}
		
		// Пример: бейдж за завершение курса
		public async Task AwardCourseCompletionBadgeAsync(int studentId)
		{
			var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Code == "COURSE_FINISHER");
			if (badge == null)
			{
				badge = new Models.Badge
				{
					Code = "COURSE_FINISHER",
					Title = "Финишер курса",
					Description = "Вы завершили курс"
				};
				_context.Badges.Add(badge);
				await _context.SaveChangesAsync();
			}
			
			var already = await _context.UserBadges.AnyAsync(ub => ub.UserId == studentId && ub.BadgeId == badge.Id);
			if (!already)
			{
				_context.UserBadges.Add(new Models.UserBadge { UserId = studentId, BadgeId = badge.Id, AwardedAt = DateTime.UtcNow });
				await _context.SaveChangesAsync();
			}
		}
	}
}





