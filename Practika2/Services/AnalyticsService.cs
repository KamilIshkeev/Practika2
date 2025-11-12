using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using System.Collections.Generic;

namespace Practika2.Services
{
	public class AnalyticsService
	{
		private readonly EduTrackContext _context;
		
		public AnalyticsService(EduTrackContext context)
		{
			_context = context;
		}
		
		public async Task<double> GetAverageCourseCompletionAsync(int courseId)
		{
			var enrollments = await _context.CourseEnrollments
				.Where(e => e.CourseId == courseId)
				.Select(e => e.Progress)
				.ToListAsync();
			if (enrollments.Count == 0) return 0;
			return enrollments.Average();
		}
		
		public async Task<int> GetCourseEnrollmentsCountAsync(int courseId)
		{
			return await _context.CourseEnrollments.CountAsync(e => e.CourseId == courseId);
		}
		
		public async Task<decimal> GetCourseRevenueAsync(int courseId)
		{
			// Простейшая модель: доход = цена * число записей на платные курсы
			var course = await _context.Courses.FindAsync(courseId);
			if (course == null || course.IsFree) return 0;
			var count = await _context.CourseEnrollments.CountAsync(e => e.CourseId == courseId);
			return course.Price * count;
		}

		public class CourseAnalytics
		{
			public int TotalEnrollments { get; set; }
			public double AverageProgress { get; set; }
			public int CompletedCourses { get; set; }
		}

		public async Task<CourseAnalytics> GetCourseAnalyticsAsync(int courseId)
		{
			var enrollments = await _context.CourseEnrollments
				.Where(e => e.CourseId == courseId)
				.ToListAsync();

			var total = enrollments.Count;
			var average = total == 0 ? 0 : enrollments.Average(e => e.Progress);
			var completed = enrollments.Count(e => e.CompletedAt.HasValue || e.Progress >= 100);

			return new CourseAnalytics
			{
				TotalEnrollments = total,
				AverageProgress = average,
				CompletedCourses = completed
			};
		}

        public class RevenueData
        {
            public string CourseName { get; set; }
            public decimal Revenue { get; set; }
        }

        public async Task<List<RevenueData>> GetRevenueByCourseAsync()
        {
            return await _context.Courses
                .Select(c => new RevenueData
                {
                    CourseName = c.Title,
                    Revenue = c.Price * c.Enrollments.Count
                })
                .ToListAsync();
        }

        public class CompletionsData
        {
            public string CourseName { get; set; }
            public int Completions { get; set; }
        }

        public async Task<List<CompletionsData>> GetCompletionsByCourseAsync()
        {
            return await _context.Courses
                .Select(c => new CompletionsData
                {
                    CourseName = c.Title,
                    Completions = c.Enrollments.Count(e => e.Progress >= 100)
                })
                .ToListAsync();
        }
	}
}




