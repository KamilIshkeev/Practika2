using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CourseStudyView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
        private readonly CourseEnrollment _enrollment;

        public CourseStudyView(EduTrackContext context, AuthService authService, 
                              CourseService courseService, EnrollmentService enrollmentService, 
                              CourseEnrollment enrollment)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            _enrollment = enrollment;
            
            LoadCourseModules();
        }

        private async void LoadCourseModules()
        {
            var course = await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == _enrollment.CourseId);
            
            if (course != null)
            {
                ModulesPanel.Children.Clear();
                foreach (var module in course.Modules.OrderBy(m => m.Order))
                {
                    var modulePanel = new StackPanel { Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                    var moduleTitle = new TextBlock 
                    { 
                        Text = module.Title,
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2E7D32")),
                        Margin = new Avalonia.Thickness(0, 0, 0, 8)
                    };
                    modulePanel.Children.Add(moduleTitle);
                    
                    foreach (var lesson in module.Lessons.OrderBy(l => l.Order))
                    {
                        var lessonButton = new Button 
                        { 
                            Content = lesson.Title,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                            Margin = new Avalonia.Thickness(16, 4, 0, 4)
                        };
                        lessonButton.Click += async (s, e) => await OnLessonClick(lesson);
                        modulePanel.Children.Add(lessonButton);
                    }
                    
                    ModulesPanel.Children.Add(modulePanel);
                }
            }
        }

        private async Task OnLessonClick(Lesson lesson)
        {
            LessonTitleTextBlock.Text = lesson.Title;
            LessonContentTextBlock.Text = lesson.Description + "\n\n" + (lesson.Content ?? "");
            
            // Удалить старую кнопку теста, если есть
            var oldTestButton = LessonContentPanel.Children.OfType<Button>().FirstOrDefault(b => b.Content?.ToString()?.Contains("тест") == true);
            if (oldTestButton != null)
            {
                LessonContentPanel.Children.Remove(oldTestButton);
            }
            
            // Обновить прогресс
            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.LessonId == lesson.Id && lp.EnrollmentId == _enrollment.Id);
            
            if (progress != null)
            {
                progress.ProgressPercentage = 100;
                progress.IsCompleted = true;
                progress.CompletedAt = System.DateTime.UtcNow;
                
                // Сохранить закладку (последний просмотренный урок)
                _enrollment.LastViewedLessonId = lesson.Id;
                
                await _context.SaveChangesAsync();
                
                await _enrollmentService.UpdateProgressAsync(_enrollment.Id);
            }
            
            // Проверить, есть ли тест к уроку
            var test = await _context.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.LessonId == lesson.Id);
            
            if (test != null && _authService.IsStudent)
            {
                // Показать кнопку для прохождения теста
                var testButton = new Button 
                { 
                    Content = "📝 Пройти тест",
                    Margin = new Avalonia.Thickness(0, 16, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                    testButton.Classes.Add("primary");
                testButton.Click += (s, e) => 
                {
                    var testWindow = new TakeTestView(_context, _authService, test, _enrollment);
                    testWindow.Show();
                };
                LessonContentPanel.Children.Add(testButton);
            }
        }
    }
}
