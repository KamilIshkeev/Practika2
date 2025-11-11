using System;
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
    public partial class CreateAnnouncementView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;

        public CreateAnnouncementView(EduTrackContext context, AuthService authService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            
            LoadCourses();
        }

        private async void LoadCourses()
        {
            if (_authService.CurrentUser == null) return;
            
            var courses = await _context.Courses
                .Include(c => c.Teachers)
                .Where(c => c.Teachers.Any(t => t.TeacherId == _authService.CurrentUser.Id))
                .ToListAsync();
            
            CourseComboBox.ItemsSource = courses;
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            if (CourseComboBox.SelectedItem is not Course course) return;
            
            var announcement = new Announcement
            {
                CourseId = course.Id,
                AuthorId = _authService.CurrentUser.Id,
                Title = TitleTextBox.Text ?? "",
                Content = ContentTextBox.Text ?? "",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            this.Close();
        }
    }
}



