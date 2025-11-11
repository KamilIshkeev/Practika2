using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CreateReviewView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly Course _course;

        public CreateReviewView(EduTrackContext context, AuthService authService, Course course)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _course = course;
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            var review = new Review
            {
                CourseId = _course.Id,
                UserId = _authService.CurrentUser.Id,
                Rating = RatingComboBox.SelectedIndex + 1,
                Comment = CommentTextBox.Text,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            
            this.Close();
        }
    }
}



