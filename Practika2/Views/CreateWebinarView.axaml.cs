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
    public partial class CreateWebinarView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;

        public CreateWebinarView(EduTrackContext context, AuthService authService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            
            LoadLessons();
        }

        private async void LoadLessons()
        {
            var lessons = await _context.Lessons
                .Where(l => l.Type == LessonType.Webinar)
                .ToListAsync();
            
            LessonComboBox.ItemsSource = lessons;
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (LessonComboBox.SelectedItem is not Lesson lesson) return;
            
            var webinar = new WebinarSession
            {
                LessonId = lesson.Id,
                Link = LinkTextBox.Text ?? "",
                ScheduledAt = ScheduledDatePicker.SelectedDate?.DateTime ?? DateTime.UtcNow.AddDays(1)
            };
            
            _context.WebinarSessions.Add(webinar);
            await _context.SaveChangesAsync();
            this.Close();
        }
    }
}



