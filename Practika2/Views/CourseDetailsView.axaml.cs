using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CourseDetailsView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
        private readonly Course _course;

        public CourseDetailsView(EduTrackContext context, AuthService authService, 
                                CourseService courseService, EnrollmentService enrollmentService, 
                                Course course)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _course = course;
            
            LoadCourseDetails();
        }

        private async void LoadCourseDetails()
        {
            CourseTitleTextBlock.Text = _course.Title;
            CourseDescriptionTextBlock.Text = _course.Description;
            RatingTextBlock.Text = $"⭐ {_course.Rating:F1} ({_course.ReviewCount} отзывов)";
            CategoryTextBlock.Text = $"Категория: {_course.Category}";
            DifficultyTextBlock.Text = $"Уровень: {_course.Difficulty}";
            PriceTextBlock.Text = _course.IsFree ? "Бесплатно" : $"{_course.Price:F0} ₽";
            
            await LoadReviews();
        }

        private async Task LoadReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseId == _course.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            
            ReviewsItemsControl.ItemsSource = reviews;
        }

        private async void OnEnrollClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            var success = await _enrollmentService.EnrollStudentAsync(_course.Id, _authService.CurrentUser.Id);
            
            if (success)
            {
                ErrorTextBlock.Text = "Вы успешно записались на курс!";
                ErrorTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4CAF50"));
                ErrorTextBlock.IsVisible = true;
                EnrollButton.IsEnabled = false;
            }
            else
            {
                ErrorTextBlock.Text = "Не удалось записаться на курс. Возможно, вы уже записаны.";
                ErrorTextBlock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E53935"));
                ErrorTextBlock.IsVisible = true;
            }
        }

        private void OnAddReviewClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            var window = new CreateReviewView(_context, _authService, _course);
            window.Show();
            window.Closed += async (s, args) => 
            {
                await LoadReviews();
                await _courseService.UpdateCourseRatingAsync(_course.Id);
                LoadCourseDetails();
            };
        }
    }
}
