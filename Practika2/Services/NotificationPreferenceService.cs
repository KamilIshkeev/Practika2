using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
	public class NotificationPreferenceService
	{
		private readonly EduTrackContext _context;
		
		public NotificationPreferenceService(EduTrackContext context)
		{
			_context = context;
		}
		
		public async Task<NotificationPreference> GetOrCreateAsync(int userId)
		{
			var p = await _context.NotificationPreferences.FirstOrDefaultAsync(x => x.UserId == userId);
			if (p != null) return p;
			p = new NotificationPreference { UserId = userId };
			_context.NotificationPreferences.Add(p);
			await _context.SaveChangesAsync();
			return p;
		}
		
		public async Task SaveAsync(NotificationPreference pref)
		{
			_context.NotificationPreferences.Update(pref);
			await _context.SaveChangesAsync();
		}
	}
}





