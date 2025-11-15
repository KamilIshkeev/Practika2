using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace Practika2.Views
{
    public partial class AdminPanelView : UserControl
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;
        private User? _selectedUser;
        public ISeries[] RevenueSeries { get; set; }
        public ISeries[] CompletionsSeries { get; set; }

        public AdminPanelView(Func<EduTrackContext> contextFactory, AuthService authService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;

            RevenueSeries = Array.Empty<ISeries>();
            CompletionsSeries = Array.Empty<ISeries>();

            DataContext = this;
            LoadData();
        }

        private async void LoadData()
        {
            await LoadAllCourses();
            await LoadUsers();

            Dispatcher.UIThread.Post(async () =>
            {
                await CalculateAnalyticsAsync();
            });
        }

        private async Task LoadAllCourses()
        {
            using var context = _contextFactory();
            var courses = await context.Courses
                .Include(c => c.Teachers)
                .ThenInclude(ct => ct.Teacher)
                .ToListAsync();

            AllCoursesItemsControl.ItemsSource = courses;
        }

        private async Task LoadUsers()
        {
            using var context = _contextFactory();
            var userManagementService = new UserManagementService(context);
            var users = await userManagementService.GetAllUsersAsync();
            if (this.FindControl<DataGrid>("UsersGrid") is DataGrid grid)
            {
                grid.ItemsSource = users;
                grid.SelectedIndex = -1;
                _selectedUser = null;
                ShowNoUserSelected();
            }
        }

        private void OnCreateCourseClick(object? sender, RoutedEventArgs e)
        {
            var createWindow = new CreateCourseView(_contextFactory, _authService);
            createWindow.Show();
            createWindow.Closed += (s, args) => LoadAllCourses();
        }

        private void OnEditCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                var editWindow = new CreateCourseView(_contextFactory, _authService, course);
                editWindow.Show();
                editWindow.Closed += (s, args) => LoadAllCourses();
            }
        }

        private async void OnDeleteCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                using var context = _contextFactory();
                var courseService = new CourseService(context);
                await courseService.DeleteCourseAsync(course.Id);
                await LoadAllCourses();
            }
        }

        private void OnAddUserClick(object? sender, RoutedEventArgs e)
        {
            var createUserView = new CreateUserView(_contextFactory);
            createUserView.Show();
            createUserView.Closed += (s, args) => LoadUsers();
        }

        private async void OnRefreshUsersClick(object? sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private async void OnBlockUserClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is User user)
            {
                using var context = _contextFactory();
                var userManagementService = new UserManagementService(context);
                await userManagementService.BlockUserAsync(user.Id, true);
                await LoadUsers();
            }
        }

        private async void OnUnblockUserClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is User user)
            {
                using var context = _contextFactory();
                var userManagementService = new UserManagementService(context);
                await userManagementService.BlockUserAsync(user.Id, false);
                await LoadUsers();
            }
        }

        private async void OnAssignTeacherClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (this.FindControl<TextBox>("AssignCourseIdTextBox") is { } courseBox &&
                    this.FindControl<TextBox>("AssignTeacherIdTextBox") is { } teacherBox &&
                    int.TryParse(courseBox.Text, out var courseId) &&
                    int.TryParse(teacherBox.Text, out var teacherId))
                {
                    using var context = _contextFactory();
                    var userManagementService = new UserManagementService(context);
                    await userManagementService.AssignTeacherToCourseAsync(courseId, teacherId);
                }
            }
            catch (Exception)
            {
                // no-op
            }
            finally
            {
                await LoadAllCourses();
            }
        }

        private async void OnCalculateAnalyticsClick(object? sender, RoutedEventArgs e)
        {
            await CalculateAnalyticsAsync();
        }

        private async Task CalculateAnalyticsAsync()
        {
            try
            {
                using var context = _contextFactory();
                var analyticsService = new AnalyticsService(context);

                var revenueData = await analyticsService.GetRevenueByCourseAsync();
                var completionsData = await analyticsService.GetCompletionsByCourseAsync();

                RevenueSeries = new[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = (revenueData?.Select(d => d.Revenue) ?? Enumerable.Empty<decimal>()).ToArray(),
                        Name = "Доход"
                    }
                };

                CompletionsSeries = new[]
                {
                    new ColumnSeries<int>
                    {
                        Values = (completionsData?.Select(d => d.Completions) ?? Enumerable.Empty<int>()).ToArray(),
                        Name = "Завершения"
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка аналитики: {ex}");
            }
        }

        private async void OnRemoveTeacherFromCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is CourseTeacher courseTeacher)
            {
                try
                {
                    using var context = _contextFactory();
                    var userManagementService = new UserManagementService(context);
                    await userManagementService.RemoveTeacherFromCourseAsync(courseTeacher.CourseId, courseTeacher.TeacherId);
                    await LoadAllCourses();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при снятии преподавателя: {ex}");
                }
            }
        }

        private async void OnUserSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is User user)
            {
                _selectedUser = user;
                await LoadUserActivityAsync();
            }
        }

        private async Task LoadUserActivityAsync()
        {
            if (_selectedUser == null)
            {
                ShowNoUserSelected();
                return;
            }

            UserActivityPanel.Children.Clear();

            // Заголовок без Classes="heading"
            var title = new TextBlock
            {
                Text = $"Активность: {_selectedUser.Username} ({_selectedUser.Role})",
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 16)
            };
            UserActivityPanel.Children.Add(title);

            using var context = _contextFactory();

            if (_selectedUser.Role == UserRole.Student)
            {
                await LoadStudentActivityAsync(context, _selectedUser.Id);
            }
            else if (_selectedUser.Role == UserRole.Teacher)
            {
                await LoadTeacherActivityAsync(context, _selectedUser.Id);
            }
            else
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "Роль не поддерживается", Foreground = Brushes.Gray });
            }
        }

        private void ShowNoUserSelected()
        {
            UserActivityPanel.Children.Clear();
            UserActivityPanel.Children.Add(new TextBlock
            {
                Text = "Выберите пользователя в таблице выше",
                Foreground = Avalonia.Media.Brushes.Gray,
                FontSize = 16
            });
        }

        private async Task LoadStudentActivityAsync(EduTrackContext context, int userId)
        {
            // 1. Записи на курсы
            var enrollments = await context.CourseEnrollments
                .Where(e => e.StudentId == userId)
                .Include(e => e.Course)
                .ToListAsync();

            if (enrollments.Any())
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "✅ Записан на курсы:", FontWeight = Avalonia.Media.FontWeight.Bold });
                var panel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                foreach (var e in enrollments)
                {
                    var progress = e.Progress >= 100 ? " (Завершён)" : $" ({e.Progress}%)";
                    panel.Children.Add(new TextBlock { Text = $"• {e.Course.Title}{progress}" });
                }
                UserActivityPanel.Children.Add(panel);
            }

            // 2. Отзывы
            var reviews = await context.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Course)
                .ToListAsync();

            if (reviews.Any())
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "📝 Отзывы:", FontWeight = Avalonia.Media.FontWeight.Bold });
                var panel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                foreach (var r in reviews)
                {
                    var comment = string.IsNullOrEmpty(r.Comment) ? "(без комментария)" : r.Comment.Length > 60 ? r.Comment[..60] + "..." : r.Comment;
                    panel.Children.Add(new TextBlock { Text = $"Курс: {r.Course.Title} | Оценка: {r.Rating} | {comment}" });
                }
                UserActivityPanel.Children.Add(panel);
            }

            // 3. Задания
            var submissions = await context.AssignmentSubmissions
                .Where(s => s.StudentId == userId)
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Lesson)
                        .ThenInclude(l => l.Module)
                            .ThenInclude(m => m.Course)
                .ToListAsync();

            if (submissions.Any())
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "📂 Задания и оценки:", FontWeight = Avalonia.Media.FontWeight.Bold });
                var panel = new StackPanel { Spacing = 8 };
                foreach (var s in submissions)
                {
                    var status = s.Points.HasValue ? $"Оценка: {s.Points}" : "На проверке";
                    panel.Children.Add(new TextBlock { Text = $"Курс: {s.Assignment.Lesson.Module.Course.Title} | Задание: {s.Assignment.Title} | {status}" });
                }
                UserActivityPanel.Children.Add(panel);
            }
        }

        private async Task LoadTeacherActivityAsync(EduTrackContext context, int teacherId)
        {
            var courseTeachers = await context.CourseTeachers
                .Where(ct => ct.TeacherId == teacherId)
                .Include(ct => ct.Course)
                .ToListAsync();

            if (courseTeachers.Any())
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "🎓 Преподаёт курсы:", FontWeight = Avalonia.Media.FontWeight.Bold });
                var panel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                foreach (var ct in courseTeachers)
                {
                    panel.Children.Add(new TextBlock { Text = $"• {ct.Course.Title}" });
                }
                UserActivityPanel.Children.Add(panel);
            }

            var courseIds = courseTeachers.Select(ct => ct.CourseId).ToList();
            if (courseIds.Any())
            {
                var reviews = await context.Reviews
                    .Where(r => courseIds.Contains(r.CourseId))
                    .Include(r => r.Course)
                    .Include(r => r.User)
                    .ToListAsync();

                if (reviews.Any())
                {
                    UserActivityPanel.Children.Add(new TextBlock { Text = "⭐ Отзывы на его курсы:", FontWeight = Avalonia.Media.FontWeight.Bold });
                    var panel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                    foreach (var r in reviews)
                    {
                        var comment = string.IsNullOrEmpty(r.Comment) ? "(без комментария)" : r.Comment.Length > 60 ? r.Comment[..60] + "..." : r.Comment;
                        panel.Children.Add(new TextBlock { Text = $"Курс: {r.Course.Title} | От: {r.User.Username} | Оценка: {r.Rating} | {comment}" });
                    }
                    UserActivityPanel.Children.Add(panel);
                }
            }

            var discussions = await context.DiscussionThreads
                .Where(t => t.AuthorId == teacherId)
                .Include(t => t.Course)
                .ToListAsync();

            if (discussions.Any())
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "💬 Созданные обсуждения:", FontWeight = Avalonia.Media.FontWeight.Bold });
                var panel = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                foreach (var d in discussions)
                {
                    panel.Children.Add(new TextBlock { Text = $"Курс: {d.Course.Title} | Тема: {d.Title}" });
                }
                UserActivityPanel.Children.Add(panel);
            }

            // 🔸 ИСПРАВЛЕНО: WebinarSession.Title → используем Lesson.Title
            var webinars = await context.WebinarSessions
                .Where(w => w.Lesson.Module.Course.Teachers.Any(ct => ct.TeacherId == teacherId))
                .Include(w => w.Lesson)
                    .ThenInclude(l => l.Module)
                        .ThenInclude(m => m.Course)
                .ToListAsync();

            if (webinars.Any())
            {
                UserActivityPanel.Children.Add(new TextBlock { Text = "🎥 Вебинары:", FontWeight = Avalonia.Media.FontWeight.Bold });
                var panel = new StackPanel { Spacing = 8 };
                foreach (var w in webinars)
                {
                    // WebinarSession не имеет Title, но у Lesson он есть
                    panel.Children.Add(new TextBlock { Text = $"Курс: {w.Lesson.Module.Course.Title} | Тема: {w.Lesson.Title}" });
                }
                UserActivityPanel.Children.Add(panel);
            }
        }

    }
}