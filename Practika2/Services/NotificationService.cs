using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
	public class NotificationService
	{
		private readonly EduTrackContext _context;
		
		public NotificationService(EduTrackContext context)
		{
			_context = context;
		}
		
		public async Task<List<Notification>> GetUnreadAsync(int userId)
		{
			return await _context.Notifications
				.Where(n => n.UserId == userId && n.Status != NotificationStatus.Read)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}
		
		public async Task MarkAsReadAsync(int notificationId)
		{
			var n = await _context.Notifications.FindAsync(notificationId);
			if (n != null)
			{
				n.Status = NotificationStatus.Read;
				n.ReadAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}
		
		public async Task EnqueueDeadlineAsync(int userId, Assignment assignment, DateTime deadlineUtc)
		{
			var n = new Notification
			{
				UserId = userId,
				Type = NotificationType.Deadline,
				Title = "Скоро дедлайн по заданию",
				Message = $"{assignment.Title}: дедлайн {deadlineUtc:g} (UTC)",
				Status = NotificationStatus.Pending,
				DueAt = deadlineUtc
			};
			_context.Notifications.Add(n);
			await _context.SaveChangesAsync();
		}
		
		public async Task EnqueueNewMaterialAsync(int userId, Lesson lesson)
		{
			var n = new Notification
			{
				UserId = userId,
				Type = NotificationType.NewMaterial,
				Title = "Новый материал",
				Message = $"Добавлен урок: {lesson.Title}",
				Status = NotificationStatus.Pending
			};
			_context.Notifications.Add(n);
			await _context.SaveChangesAsync();
		}
		
		public async Task EnqueueGradingResultAsync(int userId, AssignmentSubmission submission)
		{
			var n = new Notification
			{
				UserId = userId,
				Type = NotificationType.GradingResult,
				Title = "Оценка за задание",
				Message = $"Получено {submission.Points ?? 0} баллов",
				Status = NotificationStatus.Pending
			};
			_context.Notifications.Add(n);
			await _context.SaveChangesAsync();
		}
		
		// Простая отправка/активация: перевод Pending в Sent, если DueAt прошёл (или не задан)
		public async Task<int> DispatchDueAsync()
		{
			var now = DateTime.UtcNow;
			var due = await _context.Notifications
				.Where(n => n.Status == NotificationStatus.Pending && (n.DueAt == null || n.DueAt <= now))
				.ToListAsync();
			
			foreach (var n in due)
			{
				n.Status = NotificationStatus.Sent;
				n.SentAt = now;
			}
			
			await _context.SaveChangesAsync();
			return due.Count;
		}
	}
}





