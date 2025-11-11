using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
	public class DiscussionService
	{
		private readonly EduTrackContext _context;
		
		public DiscussionService(EduTrackContext context)
		{
			_context = context;
		}
		
		public Task<List<DiscussionThread>> GetThreadsAsync(int courseId)
		{
			return _context.DiscussionThreads
				.Where(t => t.CourseId == courseId)
				.OrderByDescending(t => t.CreatedAt)
				.ToListAsync();
		}
		
		public Task<List<DiscussionMessage>> GetMessagesAsync(int threadId)
		{
			return _context.DiscussionMessages
				.Where(m => m.ThreadId == threadId)
				.OrderBy(m => m.CreatedAt)
				.ToListAsync();
		}
		
		public async Task<DiscussionThread> CreateThreadAsync(int courseId, int authorId, string title)
		{
			var t = new DiscussionThread { CourseId = courseId, AuthorId = authorId, Title = title };
			_context.DiscussionThreads.Add(t);
			await _context.SaveChangesAsync();
			return t;
		}
		
		public async Task<DiscussionMessage> PostMessageAsync(int threadId, int authorId, string content)
		{
			var m = new DiscussionMessage { ThreadId = threadId, AuthorId = authorId, Content = content };
			_context.DiscussionMessages.Add(m);
			await _context.SaveChangesAsync();
			return m;
		}
	}
}





