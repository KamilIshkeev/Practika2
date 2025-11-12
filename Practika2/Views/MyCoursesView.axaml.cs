using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
	using System.Linq;
	using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Practika2.Views
{
    public partial class MyCoursesView : UserControl
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
			private readonly NotificationPreferenceService _prefService;
        private readonly BadgeService _badgeService;

        public MyCoursesView(EduTrackContext context, AuthService authService, 
                            CourseService courseService, EnrollmentService enrollmentService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
				_prefService = new NotificationPreferenceService(_context);
                _badgeService = new BadgeService(_context);
				
				_ = LoadPreferencesAsync();
				LoadBadges();
        }

        public async void LoadEnrollments(List<CourseEnrollment> enrollments)
        {
				var current = enrollments.Where(e => e.Progress < 100).ToList();
				var completed = enrollments.Where(e => e.Progress >= 100).ToList();
				
                foreach (var enrollment in completed)
                {
                    await _badgeService.AwardBadgeAsync(enrollment.StudentId, "COURSE_FINISHER");
                }

				EnrollmentsItemsControl.ItemsSource = current;
				CompletedItemsControl.ItemsSource = completed;
				
				NoEnrollmentsTextBlock.IsVisible = current.Count == 0;
        }

        private void OnContinueCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is CourseEnrollment enrollment)
            {
                var courseView = new CourseStudyView(_context, _authService, 
                    _courseService, _enrollmentService, enrollment);
                courseView.Show();
            }
        }

			private async Task LoadPreferencesAsync()
			{
				if (_authService.CurrentUser == null) return;
				var p = await _prefService.GetOrCreateAsync(_authService.CurrentUser.Id);
				this.FindControl<CheckBox>("PrefDeadlines").IsChecked = p.DeadlinesEnabled;
				this.FindControl<CheckBox>("PrefNewMaterials").IsChecked = p.NewMaterialsEnabled;
				this.FindControl<CheckBox>("PrefGrading").IsChecked = p.GradingResultsEnabled;
			}

			private async void OnSavePreferencesClick(object? sender, RoutedEventArgs e)
			{
				if (_authService.CurrentUser == null) return;
				var p = await _prefService.GetOrCreateAsync(_authService.CurrentUser.Id);
				p.DeadlinesEnabled = this.FindControl<CheckBox>("PrefDeadlines").IsChecked ?? true;
				p.NewMaterialsEnabled = this.FindControl<CheckBox>("PrefNewMaterials").IsChecked ?? true;
				p.GradingResultsEnabled = this.FindControl<CheckBox>("PrefGrading").IsChecked ?? true;
				await _prefService.SaveAsync(p);
			}

			private async void LoadBadges()
			{
				if (_authService.CurrentUser == null) return;
				
				var badges = await _context.UserBadges
					.Include(ub => ub.Badge)
					.Where(ub => ub.UserId == _authService.CurrentUser.Id)
					.OrderByDescending(ub => ub.AwardedAt)
					.ToListAsync();
				
				if (this.FindControl<ItemsControl>("BadgesItemsControl") is { } badgesControl)
				{
					badgesControl.ItemsSource = badges;
				}
			}
    }
}




