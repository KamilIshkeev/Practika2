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
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly TestService _testService;
        private readonly WebinarService _webinarService;
        private readonly DiscussionService _discussionService;
        private readonly AnalyticsService _analyticsService;

        public TeacherPanelView(EduTrackContext context, AuthService authService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _testService = new TestService(_context);
            _webinarService = new WebinarService(_context);
            _discussionService = new DiscussionService(_context);
            _analyticsService = new AnalyticsService(_context);
            
            LoadTeacherCourses();
            LoadSubmissions();
            LoadWebinars();
            LoadDiscussions();
            LoadAnnouncements();
        }

        private async void LoadTeacherCourses()
        {
            if (_authService.CurrentUser == null) return;
            
            using (var ctx = new EduTrackContext())
            {
                var courses = await ctx.Courses
                    .Include(c => c.Teachers)
                    .Where(c => c.Teachers.Any(t => t.TeacherId == _authService.CurrentUser.Id))
                    .ToListAsync();
            
                TeacherCoursesItemsControl.ItemsSource = courses;
            
                // Заполнить комбобоксы
                CourseSelectorComboBox.ItemsSource = null;
                CourseSelectorComboBox.Items?.Clear();
                CourseSelectorComboBox.ItemsSource = courses;
                CourseSelectorComboBox.SelectedIndex = -1;

                TestCourseSelectorComboBox.ItemsSource = null;
                TestCourseSelectorComboBox.Items?.Clear();
                TestCourseSelectorComboBox.ItemsSource = courses;
                TestCourseSelectorComboBox.SelectedIndex = -1;

                AnalyticsCourseSelectorComboBox.ItemsSource = null;
                AnalyticsCourseSelectorComboBox.Items?.Clear();
                AnalyticsCourseSelectorComboBox.ItemsSource = courses;
                AnalyticsCourseSelectorComboBox.SelectedIndex = -1;
            }
        }

        private async void LoadSubmissions()
        {
            using (var ctx = new EduTrackContext())
            {
                var submissions = await ctx.AssignmentSubmissions
                    .Include(s => s.Assignment)
                        .ThenInclude(a => a.Lesson)
                    .Include(s => s.Student)
                    .Where(s => s.Points == null)
                    .ToListAsync();
            
                SubmissionsItemsControl.ItemsSource = submissions;
            }
        }

        private async void LoadWebinars()
        {
            using (var ctx = new EduTrackContext())
            {
                var webinars = await ctx.WebinarSessions
                    .Include(w => w.Lesson)
                    .ToListAsync();
            
                WebinarsItemsControl.ItemsSource = webinars;
            }
        }

        private async void LoadDiscussions()
        {
            using (var ctx = new EduTrackContext())
            {
                var discussions = await ctx.DiscussionThreads
                    .Include(d => d.Messages)
                    .ToListAsync();
            
                DiscussionsItemsControl.ItemsSource = discussions;
            }
        }

        private async void LoadAnnouncements()
        {
            if (_authService.CurrentUser == null) return;
            
            var announcements = await _context.Announcements
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
            var lessons = await _context.Lessons
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
                var window = new CreateLessonView(_context, _authService, course.Id);
                window.Show();
                window.Closed += (s, args) => LoadLessons(course.Id);
            }
        }

        private void OnEditLessonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Lesson lesson)
            {
                var window = new CreateLessonView(_context, _authService, lesson.Module.CourseId, lesson);
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
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                
                if (CourseSelectorComboBox.SelectedItem is Course course)
                    LoadLessons(course.Id);
            }
        }

        private void OnManageCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                var window = new ManageCourseModulesView(_context, _authService, course);
                window.Show();
            }
        }

        private void OnViewAnalyticsClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                AnalyticsCourseSelectorComboBox.SelectedItem = course;
                OnAnalyticsCourseSelected(null, null);
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
            var tests = await _context.Tests
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
                var window = new CreateTestView(_context, _authService, course.Id);
                window.Show();
                window.Closed += (s, args) => LoadTests(course.Id);
            }
        }

        private void OnEditTestClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Test test)
            {
                var window = new CreateTestView(_context, _authService, 0, test);
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
                var stats = await _testService.GetTestStatisticsAsync(test.Id);
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
                    submission.Points = points;
                    submission.Comment = commentBox?.Text;
                    submission.GradedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    LoadSubmissions();
                }
            }
        }

        private void OnCreateWebinarClick(object? sender, RoutedEventArgs e)
        {
            var window = new CreateWebinarView(_context, _authService);
            window.Show();
            window.Closed += (s, args) => LoadWebinars();
        }

        private void OnOpenDiscussionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is DiscussionThread thread)
            {
                var window = new DiscussionView(_context, _authService, thread);
                window.Show();
            }
        }

        private void OnCreateAnnouncementClick(object? sender, RoutedEventArgs e)
        {
            var window = new CreateAnnouncementView(_context, _authService);
            window.Show();
            window.Closed += (s, args) => LoadAnnouncements();
        }

        private async void OnAnalyticsCourseSelected(object? sender, SelectionChangedEventArgs? e)
        {
            if (AnalyticsCourseSelectorComboBox.SelectedItem is Course course)
            {
                var stats = await _analyticsService.GetCourseAnalyticsAsync(course.Id);
                
                AnalyticsPanel.Children.Clear();
                AnalyticsPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Курс: {course.Title}",
                    FontSize = 20,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Avalonia.Thickness(0, 0, 0, 16)
                });
                
                AnalyticsPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Всего записей: {stats.TotalEnrollments}",
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                });
                
                AnalyticsPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Средний прогресс: {stats.AverageProgress:F1}%",
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                });
                
                AnalyticsPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Завершено курсов: {stats.CompletedCourses}",
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                });
            }
        }
    }
}


