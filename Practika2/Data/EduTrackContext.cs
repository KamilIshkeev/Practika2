using Microsoft.EntityFrameworkCore;
using Practika2.Models;

namespace Practika2.Data
{
    public class EduTrackContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CourseTeacher> CourseTeachers { get; set; }
		public DbSet<Test> Tests { get; set; }
		public DbSet<TestQuestion> TestQuestions { get; set; }
		public DbSet<TestAnswerOption> TestAnswerOptions { get; set; }
		public DbSet<TestSubmission> TestSubmissions { get; set; }
		public DbSet<TestSubmissionAnswer> TestSubmissionAnswers { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<Announcement> Announcements { get; set; }
		public DbSet<DiscussionThread> DiscussionThreads { get; set; }
		public DbSet<DiscussionMessage> DiscussionMessages { get; set; }
		public DbSet<Badge> Badges { get; set; }
		public DbSet<UserBadge> UserBadges { get; set; }
		public DbSet<CourseCertificate> CourseCertificates { get; set; }
		public DbSet<WebinarSession> WebinarSessions { get; set; }
		public DbSet<NotificationPreference> NotificationPreferences { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // PostgreSQL connection (adjust credentials if needed)
            var connectionString = "Host=localhost;Port=5432;Database=practika2;Username=postgres;Password=1108";
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Course>()
                .HasOne(c => c.CreatedBy)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Lesson)
                .WithMany(l => l.Assignments)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseEnrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LessonProgress>()
                .HasOne(p => p.Lesson)
                .WithMany(l => l.Progresses)
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonProgress>()
                .HasOne(p => p.Enrollment)
                .WithMany(e => e.LessonProgresses)
                .HasForeignKey(p => p.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseTeacher>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.Teachers)
                .HasForeignKey(ct => ct.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseTeacher>()
                .HasOne(ct => ct.Teacher)
                .WithMany()
                .HasForeignKey(ct => ct.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<CourseEnrollment>()
                .HasIndex(e => new { e.CourseId, e.StudentId })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.CourseId, r.UserId })
                .IsUnique();

            modelBuilder.Entity<CourseTeacher>()
                .HasIndex(ct => new { ct.CourseId, ct.TeacherId })
                .IsUnique();
			
			// Tests
			modelBuilder.Entity<Test>()
				.HasOne(t => t.Lesson)
				.WithMany() // у Lesson нет коллекции тестов
				.HasForeignKey(t => t.LessonId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<TestQuestion>()
				.HasOne(q => q.Test)
				.WithMany(t => t.Questions)
				.HasForeignKey(q => q.TestId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<TestAnswerOption>()
				.HasOne(o => o.Question)
				.WithMany(q => q.Options)
				.HasForeignKey(o => o.QuestionId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<TestSubmission>()
				.HasOne(s => s.Test)
				.WithMany(t => t.Submissions)
				.HasForeignKey(s => s.TestId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<TestSubmission>()
				.HasOne(s => s.Student)
				.WithMany()
				.HasForeignKey(s => s.StudentId)
				.OnDelete(DeleteBehavior.Restrict);
			
			modelBuilder.Entity<TestSubmissionAnswer>()
				.HasOne(a => a.Submission)
				.WithMany(s => s.Answers)
				.HasForeignKey(a => a.SubmissionId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<TestSubmissionAnswer>()
				.HasOne(a => a.Question)
				.WithMany()
				.HasForeignKey(a => a.QuestionId)
				.OnDelete(DeleteBehavior.Restrict);
			
			modelBuilder.Entity<TestSubmissionAnswer>()
				.HasOne(a => a.SelectedOption)
				.WithMany()
				.HasForeignKey(a => a.SelectedOptionId)
				.OnDelete(DeleteBehavior.Restrict);
			
			// Notifications
			modelBuilder.Entity<Notification>()
				.HasOne(n => n.User)
				.WithMany()
				.HasForeignKey(n => n.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// Announcements
			modelBuilder.Entity<Announcement>()
				.HasOne(a => a.Course)
				.WithMany()
				.HasForeignKey(a => a.CourseId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<Announcement>()
				.HasOne(a => a.Author)
				.WithMany()
				.HasForeignKey(a => a.AuthorId)
				.OnDelete(DeleteBehavior.Restrict);
			
			// Discussions
			modelBuilder.Entity<DiscussionThread>()
				.HasOne(t => t.Course)
				.WithMany()
				.HasForeignKey(t => t.CourseId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<DiscussionThread>()
				.HasOne(t => t.Author)
				.WithMany()
				.HasForeignKey(t => t.AuthorId)
				.OnDelete(DeleteBehavior.Restrict);
			
			modelBuilder.Entity<DiscussionMessage>()
				.HasOne(m => m.Thread)
				.WithMany(t => t.Messages)
				.HasForeignKey(m => m.ThreadId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<DiscussionMessage>()
				.HasOne(m => m.Author)
				.WithMany()
				.HasForeignKey(m => m.AuthorId)
				.OnDelete(DeleteBehavior.Restrict);
			
			// Badges
			modelBuilder.Entity<UserBadge>()
				.HasOne(ub => ub.User)
				.WithMany() // нет коллекции бейджей у User
				.HasForeignKey(ub => ub.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<UserBadge>()
				.HasOne(ub => ub.Badge)
				.WithMany(b => b.AwardedTo)
				.HasForeignKey(ub => ub.BadgeId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// Certificates
			modelBuilder.Entity<CourseCertificate>()
				.HasOne(cc => cc.Course)
				.WithMany()
				.HasForeignKey(cc => cc.CourseId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<CourseCertificate>()
				.HasOne(cc => cc.Student)
				.WithMany()
				.HasForeignKey(cc => cc.StudentId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// Webinars
			modelBuilder.Entity<WebinarSession>()
				.HasOne(w => w.Lesson)
				.WithMany()
				.HasForeignKey(w => w.LessonId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// Notification preferences
			modelBuilder.Entity<NotificationPreference>()
				.HasOne(p => p.User)
				.WithMany()
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<NotificationPreference>()
				.HasIndex(p => p.UserId)
				.IsUnique();
        }
    }
}




