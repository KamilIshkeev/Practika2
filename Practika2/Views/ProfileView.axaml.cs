using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.Generic;
using System;

namespace Practika2.Views
{
    public partial class ProfileView : UserControl
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;
        private User _currentUser;

        public ISeries[] OverallProgressSeries { get; set; }
        public List<CourseProgressViewModel> CourseProgressSeries { get; set; }

        public ProfileView(Func<EduTrackContext> contextFactory, AuthService authService)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;

            _currentUser = _authService.CurrentUser!;
            LoadUserData();
            LoadChartData();

            this.FindControl<Button>("ChangePhotoButton")!.Click += OnChangePhotoButtonClick;
            this.FindControl<Button>("SaveChangesButton")!.Click += OnSaveChangesButtonClick;

            DataContext = this;
        }

        private void LoadUserData()
        {
            if (_currentUser == null) return;

            this.FindControl<TextBox>("FirstNameTextBox")!.Text = _currentUser.FirstName;
            this.FindControl<TextBox>("LastNameTextBox")!.Text = _currentUser.LastName;
            this.FindControl<TextBox>("EmailTextBox")!.Text = _currentUser.Email;

            if (!string.IsNullOrEmpty(_currentUser.ProfileImagePath) && File.Exists(_currentUser.ProfileImagePath))
            {
                this.FindControl<Image>("ProfileImage")!.Source = new Bitmap(_currentUser.ProfileImagePath);
            }
        }

        private void LoadChartData()
        {
            using var context = _contextFactory();
            var enrollments = context.CourseEnrollments
                .Where(e => e.StudentId == _currentUser.Id)
                .Include(e => e.Course)
                .ToList();

            var overallProgress = enrollments.Any() ? enrollments.Average(e => e.Progress) : 0;
            OverallProgressSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { overallProgress },
                    Name = "Общий прогресс"
                }
            };

            CourseProgressSeries = new List<CourseProgressViewModel>();
            foreach (var enrollment in enrollments)
            {
                CourseProgressSeries.Add(new CourseProgressViewModel
                {
                    CourseName = enrollment.Course.Title,
                    Series = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Values = new double[] { enrollment.Progress },
                            Name = enrollment.Course.Title
                        }
                    }
                });
            }
        }

        private async void OnChangePhotoButtonClick(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение профиля",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Image Files", Extensions = new List<string> { "jpg", "jpeg", "png", "bmp" } }
                }
            };

            var result = await openFileDialog.ShowAsync(this.VisualRoot as Window);

            if (result != null && result.Length > 0)
            {
                var imagePath = result[0];
                _currentUser.ProfileImagePath = imagePath;
                this.FindControl<Image>("ProfileImage")!.Source = new Bitmap(imagePath);
            }
        }

        private async void OnSaveChangesButtonClick(object? sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            using var context = _contextFactory();
            var userService = new UserService(context);

            _currentUser.FirstName = this.FindControl<TextBox>("FirstNameTextBox")!.Text ?? "";
            _currentUser.LastName = this.FindControl<TextBox>("LastNameTextBox")!.Text ?? "";
            _currentUser.Email = this.FindControl<TextBox>("EmailTextBox")!.Text ?? "";

            await userService.UpdateUserAsync(_currentUser);

            var newPassword = this.FindControl<TextBox>("NewPasswordTextBox")!.Text;
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                await userService.ChangePasswordAsync(_currentUser, newPassword);
            }
        }
    }

    public class CourseProgressViewModel
    {
        public string CourseName { get; set; }
        public ISeries[] Series { get; set; }
    }
}