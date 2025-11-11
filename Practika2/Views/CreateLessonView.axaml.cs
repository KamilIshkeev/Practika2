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
    public partial class CreateLessonView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly int _courseId;
        private readonly Lesson? _existingLesson;

        public CreateLessonView(EduTrackContext context, AuthService authService, int courseId, Lesson? existingLesson = null)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseId = courseId;
            _existingLesson = existingLesson;
            
            if (_existingLesson != null)
            {
                LoadLessonData();
            }
        }

        private void LoadLessonData()
        {
            if (_existingLesson == null) return;
            
            TitleTextBox.Text = _existingLesson.Title;
            DescriptionTextBox.Text = _existingLesson.Description;
            TypeComboBox.SelectedIndex = (int)_existingLesson.Type;
            ContentUrlTextBox.Text = _existingLesson.ContentUrl ?? "";
            ContentTextBox.Text = _existingLesson.Content ?? "";
            DurationTextBox.Text = _existingLesson.Duration.ToString();
            OrderTextBox.Text = _existingLesson.Order.ToString();
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            var module = await _context.Modules
                .Where(m => m.CourseId == _courseId)
                .OrderBy(m => m.Order)
                .FirstOrDefaultAsync();
            
            if (module == null)
            {
                // Создать модуль по умолчанию
                module = new Module
                {
                    Title = "Модуль 1",
                    Description = "Основной модуль",
                    Order = 1,
                    CourseId = _courseId
                };
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();
            }
            
            var lesson = _existingLesson ?? new Lesson();
            lesson.Title = TitleTextBox.Text ?? "";
            lesson.Description = DescriptionTextBox.Text ?? "";
            lesson.Type = (LessonType)TypeComboBox.SelectedIndex;
            lesson.ContentUrl = string.IsNullOrWhiteSpace(ContentUrlTextBox.Text) ? null : ContentUrlTextBox.Text;
            lesson.Content = string.IsNullOrWhiteSpace(ContentTextBox.Text) ? null : ContentTextBox.Text;
            lesson.Duration = int.TryParse(DurationTextBox.Text, out var duration) ? duration : 0;
            lesson.Order = int.TryParse(OrderTextBox.Text, out var order) ? order : 0;
            lesson.ModuleId = module.Id;
            
            if (_existingLesson == null)
            {
                _context.Lessons.Add(lesson);
            }
            
            await _context.SaveChangesAsync();
            this.Close();
        }
    }
}



