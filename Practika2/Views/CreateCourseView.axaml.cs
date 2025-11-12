using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CreateCourseView : Window
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;
        private readonly Course? _existingCourse;

        public CreateCourseView(Func<EduTrackContext> contextFactory, AuthService authService, Course? existingCourse = null)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;
            _existingCourse = existingCourse;
            
            if (_existingCourse != null)
            {
                LoadCourseData();
            }
        }

        private void LoadCourseData()
        {
            if (_existingCourse == null) return;
            
            TitleTextBox.Text = _existingCourse.Title;
            DescriptionTextBox.Text = _existingCourse.Description;
            CategoryComboBox.SelectedIndex = (int)_existingCourse.Category;
            DifficultyComboBox.SelectedIndex = (int)_existingCourse.Difficulty;
            DurationTextBox.Text = _existingCourse.Duration.ToString();
            PriceTextBox.Text = _existingCourse.Price.ToString();
            FormatComboBox.SelectedIndex = (int)_existingCourse.Format;
            CoverImagePathTextBox.Text = _existingCourse.CoverImagePath ?? "";
            IsPublishedCheckBox.IsChecked = _existingCourse.IsPublished;
            IsArchivedCheckBox.IsChecked = _existingCourse.IsArchived;
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            using var context = _contextFactory();
            var courseService = new CourseService(context);

            var course = _existingCourse ?? new Course();
            course.Title = TitleTextBox.Text ?? "";
            course.Description = DescriptionTextBox.Text ?? "";
            course.Category = (CourseCategory)CategoryComboBox.SelectedIndex;
            course.Difficulty = (DifficultyLevel)DifficultyComboBox.SelectedIndex;
            course.Duration = int.TryParse(DurationTextBox.Text, out var duration) ? duration : 0;
            course.Price = decimal.TryParse(PriceTextBox.Text, out var price) ? price : 0;
            course.Format = (CourseFormat)FormatComboBox.SelectedIndex;
            course.CoverImagePath = string.IsNullOrWhiteSpace(CoverImagePathTextBox.Text) ? null : CoverImagePathTextBox.Text;
            course.IsPublished = IsPublishedCheckBox.IsChecked ?? false;
            course.IsArchived = IsArchivedCheckBox.IsChecked ?? false;
            
            if (_existingCourse == null)
            {
                course.CreatedById = _authService.CurrentUser.Id;
                course.CreatedAt = DateTime.UtcNow;
                await courseService.CreateCourseAsync(course);
            }
            else
            {
                course.UpdatedAt = DateTime.UtcNow;
                await courseService.UpdateCourseAsync(course);
            }
            
            this.Close();
        }
    }
}