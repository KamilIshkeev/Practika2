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

namespace Practika2.Views
{
    public partial class AdminPanelView : UserControl
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
			private readonly UserManagementService _userManagementService;
			private readonly AnalyticsService _analyticsService;

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
        }

        private async void LoadAllCourses()
        {
            var courses = await _courseService.GetCoursesAsync(publishedOnly: false);
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

			private async void OnCalculateAnalyticsClick(object? sender, RoutedEventArgs e)
			{
				if (this.FindControl<TextBox>("AnalyticsCourseIdTextBox") is { } box &&
				    int.TryParse(box.Text, out var courseId))
				{
					var count = await _analyticsService.GetCourseEnrollmentsCountAsync(courseId);
					var avg = await _analyticsService.GetAverageCourseCompletionAsync(courseId);
					var revenue = await _analyticsService.GetCourseRevenueAsync(courseId);

					if (this.FindControl<TextBlock>("AnalyticsEnrollmentsText") is { } t1) t1.Text = $"Записей: {count}";
					if (this.FindControl<TextBlock>("AnalyticsCompletionText") is { } t2) t2.Text = $"Средний прогресс: {avg:F1}%";
					if (this.FindControl<TextBlock>("AnalyticsRevenueText") is { } t3) t3.Text = $"Доход: {revenue:F0}₽";
				}
			}
    }
}




