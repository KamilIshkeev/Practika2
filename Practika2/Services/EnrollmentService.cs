using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
    public class EnrollmentService
    {
        private readonly EduTrackContext _context;

        public EnrollmentService(EduTrackContext context)
        {
            _context = context;
        }

        public async Task<bool> EnrollStudentAsync(int courseId, int studentId)
        {
            if (await _context.CourseEnrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId))
                return false;

            var enrollment = new CourseEnrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow,
                Progress = 0
            };

            _context.CourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Initialize lesson progresses
            var course = await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course != null)
            {
                foreach (var module in course.Modules)
                {
                    foreach (var lesson in module.Lessons)
                    {
                        var progress = new LessonProgress
                        {
                            LessonId = lesson.Id,
                            EnrollmentId = enrollment.Id,
                            IsCompleted = false,
                            ProgressPercentage = 0
                        };
                        _context.LessonProgresses.Add(progress);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<CourseEnrollment?> GetEnrollmentAsync(int courseId, int studentId)
        {
            return await _context.CourseEnrollments
                .Include(e => e.Course)
                .Include(e => e.LessonProgresses)
                    .ThenInclude(lp => lp.Lesson)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
        }

        public async Task UpdateProgressAsync(int enrollmentId)
        {
            var enrollment = await _context.CourseEnrollments
                .Include(e => e.LessonProgresses)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment != null && enrollment.LessonProgresses.Any())
            {
                var completedCount = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);
                var totalCount = enrollment.LessonProgresses.Count;
                enrollment.Progress = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0;

                if (enrollment.Progress == 100)
                    enrollment.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CourseEnrollment>> GetStudentEnrollmentsAsync(int studentId)
        {
            return await _context.CourseEnrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }
    }
}

