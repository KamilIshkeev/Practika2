using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class TeacherPanelView : UserControl
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;

        public TeacherPanelView(Func<EduTrackContext> contextFactory, AuthService authService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;
            
            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            await LoadTeacherCourses();
            await LoadSubmissions();
            await LoadWebinars();
            await LoadDiscussions();
            await LoadAnnouncements();
        }

        private async Task LoadTeacherCourses()
        {
            if (_authService.CurrentUser == null) return;
            
            using var context = _contextFactory();
            var courses = await context.Courses
                .Include(c => c.Teachers)
                .Where(c => c.Teachers.Any(t => t.TeacherId == _authService.CurrentUser.Id))
                .ToListAsync();

            TeacherCoursesItemsControl.ItemsSource = courses;
            
            CourseSelectorComboBox.ItemsSource = courses;
            TestCourseSelectorComboBox.ItemsSource = courses;
            AnalyticsCourseSelectorComboBox.ItemsSource = courses;
        }

        private async Task LoadSubmissions()
        {
            using var context = _contextFactory();
            var submissions = await context.AssignmentSubmissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                .Include(s => s.Student)
                .ToListAsync();

            SubmissionsItemsControl.ItemsSource = submissions;
        }

        private async Task LoadWebinars()
        {
            using var context = _contextFactory();
            var webinars = await context.WebinarSessions
                .Include(w => w.Lesson)
                .ToListAsync();

            WebinarsItemsControl.ItemsSource = webinars;
        }

        private async Task LoadDiscussions()
        {
            using var context = _contextFactory();
            var discussions = await context.DiscussionThreads
                .Include(d => d.Messages)
                .ToListAsync();

            DiscussionsItemsControl.ItemsSource = discussions;
        }

        private async Task LoadAnnouncements()
        {
            if (_authService.CurrentUser == null) return;
            
            using var context = _contextFactory();
            var announcements = await context.Announcements
                .Where(a => a.AuthorId == _authService.CurrentUser.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            
            AnnouncementsItemsControl.ItemsSource = announcements;
        }

        private void OnCourseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (CourseSelectorComboBox.SelectedItem is Course course)
            {
                AddLessonButton.IsEnabled = true;
                LoadLessons(course.Id);
            }
            else
            {
                AddLessonButton.IsEnabled = false;
            }
        }

        private async void LoadLessons(int courseId)
        {
            using var context = _contextFactory();
            var lessons = await context.Lessons
                .Include(l => l.Module)
                .Where(l => l.Module.CourseId == courseId)
                .OrderBy(l => l.Module.Order)
                .ThenBy(l => l.Order)
                .ToListAsync();
            
            LessonsItemsControl.ItemsSource = lessons;
        }

        private void OnAddLessonClick(object? sender, RoutedEventArgs e)
        {
            if (CourseSelectorComboBox.SelectedItem is Course course)
            {
                var window = new CreateLessonView(_contextFactory, _authService, course.Id);
                window.Show();
                window.Closed += (s, args) => LoadLessons(course.Id);
            }
        }

        private void OnEditLessonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Lesson lesson)
            {
                var window = new CreateLessonView(_contextFactory, _authService, lesson.Module.CourseId, lesson);
                window.Show();
                window.Closed += (s, args) => 
                {
                    if (CourseSelectorComboBox.SelectedItem is Course course)
                        LoadLessons(course.Id);
                };
            }
        }

        private async void OnDeleteLessonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Lesson lesson)
            {
                using var context = _contextFactory();
                context.Lessons.Remove(lesson);
                await context.SaveChangesAsync();
                
                if (CourseSelectorComboBox.SelectedItem is Course course)
                    LoadLessons(course.Id);
            }
        }

        private void OnManageCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                var window = new ManageCourseModulesView(_contextFactory, _authService, course);
                window.Show();
            }
        }

        private void OnViewAnalyticsClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                AnalyticsCourseSelectorComboBox.SelectedItem = course;
                OnAnalyticsCourseSelected(null, null);

                // Switch to the Analytics Tab
                var tabControl = this.FindControl<TabControl>("MainTabControl");
                if (tabControl != null)
                {
                    tabControl.SelectedIndex = 5; // Assuming "Аналитика курса" is the 6th tab (index 5)
                }
            }
        }

        private void OnTestCourseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (TestCourseSelectorComboBox.SelectedItem is Course course)
            {
                CreateTestButton.IsEnabled = true;
                LoadTests(course.Id);
            }
            else
            {
                CreateTestButton.IsEnabled = false;
            }
        }

        private async void LoadTests(int courseId)
        {
            using var context = _contextFactory();
            var tests = await context.Tests
                .Include(t => t.Lesson)
                    .ThenInclude(l => l.Module)
                .Where(t => t.Lesson.Module.CourseId == courseId)
                .ToListAsync();
            
            TestsItemsControl.ItemsSource = tests;
        }

        private void OnCreateTestClick(object? sender, RoutedEventArgs e)
        {
            if (TestCourseSelectorComboBox.SelectedItem is Course course)
            {
                var window = new CreateTestView(_contextFactory, _authService, course.Id);
                window.Show();
                window.Closed += (s, args) => LoadTests(course.Id);
            }
        }

        private void OnEditTestClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Test test)
            {
                var window = new CreateTestView(_contextFactory, _authService, 0, test);
                window.Show();
                window.Closed += (s, args) => 
                {
                    if (TestCourseSelectorComboBox.SelectedItem is Course course)
                        LoadTests(course.Id);
                };
            }
        }

        private async void OnTestStatsClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Test test)
            {
                using var context = _contextFactory();
                var testService = new TestService(context);
                var stats = await testService.GetTestStatisticsAsync(test.Id);
                var window = new TestStatisticsView(stats);
                window.Show();
            }
        }

        private async void OnGradeSubmissionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is AssignmentSubmission submission)
            {
                var border = button.Parent as StackPanel;
                var pointsBox = border?.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "PointsTextBox");
                var commentBox = border?.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "CommentTextBox");
                
                if (pointsBox != null && int.TryParse(pointsBox.Text, out var points))
                {
                    using var context = _contextFactory();
                    var badgeService = new BadgeService(context);

                    submission.Points = points;
                    submission.Comment = commentBox?.Text;
                    submission.GradedAt = DateTime.UtcNow;
                    context.AssignmentSubmissions.Update(submission);
                    await context.SaveChangesAsync();
                    
                    var submissionCount = await context.AssignmentSubmissions
                        .CountAsync(s => s.StudentId == submission.StudentId && s.Points != null);
                    if (submissionCount == 1)
                    {
                        await badgeService.AwardBadgeAsync(submission.StudentId, "FIRST_SUBMISSION");
                    }

                    await LoadSubmissions();
                }
            }
        }

        private void OnCreateWebinarClick(object? sender, RoutedEventArgs e)
        {
            var window = new CreateWebinarView(_contextFactory, _authService);
            window.Show();
            window.Closed += async (s, args) => await LoadWebinars();
        }

        private void OnOpenDiscussionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is DiscussionThread thread)
            {
                var window = new DiscussionView(_contextFactory, _authService, thread);
                window.Show();
            }
        }

        private void OnCreateAnnouncementClick(object? sender, RoutedEventArgs e)
        {
            var window = new CreateAnnouncementView(_contextFactory, _authService);
            window.Show();
            window.Closed += async (s, args) => await LoadAnnouncements();
        }

        private async void OnAnalyticsCourseSelected(object? sender, SelectionChangedEventArgs? e)
        {
            if (AnalyticsCourseSelectorComboBox.SelectedItem is Course course)
            {
                using var context = _contextFactory();
                var enrollments = await context.CourseEnrollments
                    .Include(en => en.Student)
                    .Where(en => en.CourseId == course.Id)
                    .ToListAsync();
                
                var grid = this.FindControl<DataGrid>("AnalyticsGrid");
                if (grid != null)
                {
                    grid.ItemsSource = enrollments;
                }
            }
        }

        private void OnRefreshAnalyticsClick(object? sender, RoutedEventArgs e)
        {
            OnAnalyticsCourseSelected(null, null);
        }
    }
}