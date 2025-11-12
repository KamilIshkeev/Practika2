using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CreateReviewView : Window
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;
        private readonly Course _course;

        public CreateReviewView(Func<EduTrackContext> contextFactory, AuthService authService, Course course)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;
            _course = course;
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            using var context = _contextFactory();

            var review = new Review
            {
                CourseId = _course.Id,
                UserId = _authService.CurrentUser.Id,
                Rating = RatingComboBox.SelectedIndex + 1,
                Comment = CommentTextBox.Text,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Reviews.Add(review);
            await context.SaveChangesAsync();
            
            this.Close();
        }
    }
}