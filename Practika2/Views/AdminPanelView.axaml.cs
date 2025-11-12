using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
	using System;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Practika2.Views
{
    public partial class AdminPanelView : UserControl
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
			private readonly UserManagementService _userManagementService;
			private readonly AnalyticsService _analyticsService;

        public ISeries[] RevenueSeries { get; set; }
        public ISeries[] CompletionsSeries { get; set; }

        public AdminPanelView(EduTrackContext context, AuthService authService, CourseService courseService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseService = courseService;
				_userManagementService = new UserManagementService(_context);
				_analyticsService = new AnalyticsService(_context);
            
            LoadAllCourses();
				LoadUsers();
            OnCalculateAnalyticsClick(null, null);
            DataContext = this;
        }

        private async void LoadAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Teachers)
                .ThenInclude(ct => ct.Teacher)
                .ToListAsync();
            AllCoursesItemsControl.ItemsSource = courses;
        }

			private async void LoadUsers()
			{
				var users = await _userManagementService.GetAllUsersAsync();
				if (this.FindControl<DataGrid>("UsersGrid") is { } grid)
				{
					grid.ItemsSource = users;
				}
			}

        private void OnCreateCourseClick(object? sender, RoutedEventArgs e)
        {
            var createWindow = new CreateCourseView(_context, _authService, _courseService);
            createWindow.Show();
            createWindow.Closed += (s, args) => LoadAllCourses();
        }

        private void OnEditCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                var editWindow = new CreateCourseView(_context, _authService, _courseService, course);
                editWindow.Show();
                editWindow.Closed += (s, args) => LoadAllCourses();
            }
        }

        private async void OnDeleteCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                await _courseService.DeleteCourseAsync(course.Id);
                LoadAllCourses();
            }
        }

        private void OnAddUserClick(object? sender, RoutedEventArgs e)
        {
            var createUserView = new CreateUserView(_context);
            createUserView.Show();
            createUserView.Closed += (s, args) => LoadUsers();
        }

			private void OnRefreshUsersClick(object? sender, RoutedEventArgs e)
			{
				LoadUsers();
			}

			private async void OnBlockUserClick(object? sender, RoutedEventArgs e)
			{
				if (sender is Button button && button.CommandParameter is User user)
				{
					await _userManagementService.BlockUserAsync(user.Id, true);
					LoadUsers();
				}
			}

			private async void OnUnblockUserClick(object? sender, RoutedEventArgs e)
			{
				if (sender is Button button && button.CommandParameter is User user)
				{
					await _userManagementService.BlockUserAsync(user.Id, false);
					LoadUsers();
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
						await _userManagementService.AssignTeacherToCourseAsync(courseId, teacherId);
					}
				}
				catch (Exception)
				{
					// no-op
				}
				finally
				{
					LoadAllCourses();
				}
			}

			private async void OnCalculateAnalyticsClick(object? sender, RoutedEventArgs? e)
			{
                var revenueData = await _analyticsService.GetRevenueByCourseAsync();
                RevenueSeries = new ISeries[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = revenueData.Select(d => d.Revenue).ToArray(),
                        Name = "Доход"
                    }
                };

                var completionsData = await _analyticsService.GetCompletionsByCourseAsync();
                CompletionsSeries = new ISeries[]
                {
                    new ColumnSeries<int>
                    {
                        Values = completionsData.Select(d => d.Completions).ToArray(),
                        Name = "Завершения"
                    }
                };

                // This is a bit of a hack to force the UI to update.
                // A better solution would be to use an MVVM framework.
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




