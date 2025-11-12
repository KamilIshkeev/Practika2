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
    public partial class MyCoursesView : UserControl
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;

        public MyCoursesView(Func<EduTrackContext> contextFactory, AuthService authService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;

            LoadData();
        }

        private async void LoadData()
        {
            await LoadPreferencesAsync();
            await LoadBadges();
        }

        public async void LoadEnrollments(List<CourseEnrollment> enrollments)
        {
            using var context = _contextFactory();
            var badgeService = new BadgeService(context);

            var current = enrollments.Where(e => e.Progress < 100).ToList();
            var completed = enrollments.Where(e => e.Progress >= 100).ToList();

            foreach (var enrollment in completed)
            {
                await badgeService.AwardBadgeAsync(enrollment.StudentId, "COURSE_FINISHER");
            }

            EnrollmentsItemsControl.ItemsSource = current;
            CompletedItemsControl.ItemsSource = completed;

            NoEnrollmentsTextBlock.IsVisible = current.Count == 0;
        }

        private void OnContinueCourseClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is CourseEnrollment enrollment)
            {
                var courseView = new CourseStudyView(_contextFactory, _authService, enrollment);
                courseView.Show();
            }
        }

        private async Task LoadPreferencesAsync()
        {
            if (_authService.CurrentUser == null) return;

            using var context = _contextFactory();
            var prefService = new NotificationPreferenceService(context);

            var p = await prefService.GetOrCreateAsync(_authService.CurrentUser.Id);
            this.FindControl<CheckBox>("PrefDeadlines").IsChecked = p.DeadlinesEnabled;
            this.FindControl<CheckBox>("PrefNewMaterials").IsChecked = p.NewMaterialsEnabled;
            this.FindControl<CheckBox>("PrefGrading").IsChecked = p.GradingResultsEnabled;
        }

        private async void OnSavePreferencesClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;

            using var context = _contextFactory();
            var prefService = new NotificationPreferenceService(context);

            var p = await prefService.GetOrCreateAsync(_authService.CurrentUser.Id);
            p.DeadlinesEnabled = this.FindControl<CheckBox>("PrefDeadlines").IsChecked ?? true;
            p.NewMaterialsEnabled = this.FindControl<CheckBox>("PrefNewMaterials").IsChecked ?? true;
            p.GradingResultsEnabled = this.FindControl<CheckBox>("PrefGrading").IsChecked ?? true;
            await prefService.SaveAsync(p);
        }

        private async Task LoadBadges()
        {
            if (_authService.CurrentUser == null) return;

            using var context = _contextFactory();

            var badges = await context.UserBadges
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