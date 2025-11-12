using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
using System.Threading.Tasks;

namespace Practika2.Views
{
    public partial class AdminPanelView : UserControl
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;

        public ISeries[] RevenueSeries { get; set; }
        public ISeries[] CompletionsSeries { get; set; }

        public AdminPanelView(Func<EduTrackContext> contextFactory, AuthService authService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;

            DataContext = this;

            LoadData();
        }

        private async void LoadData()
        {
            await LoadAllCourses();
            await LoadUsers();
            await OnCalculateAnalyticsClick();
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
            if (this.FindControl<DataGrid>("UsersGrid") is { } grid)
            {
                grid.ItemsSource = users;
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

        private async Task OnCalculateAnalyticsClick()
        {
            using var context = _contextFactory();
            var analyticsService = new AnalyticsService(context);

            var revenueData = await analyticsService.GetRevenueByCourseAsync();
            RevenueSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Values = revenueData.Select(d => d.Revenue).ToArray(),
                    Name = "Доход"
                }
            };

            var completionsData = await analyticsService.GetCompletionsByCourseAsync();
            CompletionsSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = completionsData.Select(d => d.Completions).ToArray(),
                    Name = "Завершения"
                }
            };

            var grid = this.FindControl<DataGrid>("UsersGrid");
            if (grid != null)
            {
                var items = grid.ItemsSource;
                grid.ItemsSource = null;
                grid.ItemsSource = items;
            }
        }
    }
}