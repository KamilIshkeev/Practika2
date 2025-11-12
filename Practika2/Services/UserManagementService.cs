using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using System;

namespace Practika2.Services
{
	public class UserManagementService
	{
		private readonly EduTrackContext _context;
		
		public UserManagementService(EduTrackContext context)
		{
			_context = context;
		}
		
		public Task<List<User>> GetAllUsersAsync() =>
			_context.Users.OrderBy(u => u.Username).ToListAsync();
		
		public async Task BlockUserAsync(int userId, bool isBlocked)
		{
			var u = await _context.Users.FindAsync(userId);
			if (u != null)
			{
				u.IsBlocked = isBlocked;
				await _context.SaveChangesAsync();
			}
		}
		
		public async Task AssignTeacherToCourseAsync(int courseId, int teacherId)
		{
			var exists = await _context.CourseTeachers.AnyAsync(ct => ct.CourseId == courseId && ct.TeacherId == teacherId);
			if (!exists)
			{
				_context.CourseTeachers.Add(new CourseTeacher
				{
					CourseId = courseId,
					TeacherId = teacherId
				});
				await _context.SaveChangesAsync();
			}
		}
		
		public async Task RemoveTeacherFromCourseAsync(int courseId, int teacherId)
		{
			var entity = await _context.CourseTeachers
				.FirstOrDefaultAsync(ct => ct.CourseId == courseId && ct.TeacherId == teacherId);
			if (entity != null)
			{
				_context.CourseTeachers.Remove(entity);
				await _context.SaveChangesAsync();
			}
		}

		public async Task CreateUserAsync(string username, string email, string password, string firstName, string lastName, UserRole role)
		{
			var user = new User
			{
				Username = username,
				Email = email,
				PasswordHash = password, // Plain text as per project requirements
				FirstName = firstName,
				LastName = lastName,
				Role = role,
				CreatedAt = DateTime.UtcNow
			};
			_context.Users.Add(user);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateUserAsync(User user)
		{
			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}
	}
}





