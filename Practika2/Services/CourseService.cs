using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
    public class CourseService
    {
        private readonly EduTrackContext _context;

        public CourseService(EduTrackContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetCoursesAsync(bool publishedOnly = false)
        {
            var query = _context.Courses.Include(c => c.CreatedBy).AsQueryable();
            
            if (publishedOnly)
                query = query.Where(c => c.IsPublished && !c.IsArchived);
            
            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.CreatedBy)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .Include(c => c.Teachers)
                    .ThenInclude(ct => ct.Teacher)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            course.UpdatedAt = DateTime.UtcNow;
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ArchiveCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsArchived = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Course>> GetCoursesByCategoryAsync(CourseCategory category)
        {
            return await _context.Courses
                .Where(c => c.Category == category && c.IsPublished && !c.IsArchived)
                .ToListAsync();
        }

        public async Task UpdateCourseRatingAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course != null && course.Reviews.Any())
            {
                course.Rating = course.Reviews.Average(r => r.Rating);
                course.ReviewCount = course.Reviews.Count;
                await _context.SaveChangesAsync();
            }
        }
    }
}

