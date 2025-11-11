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
    public partial class ManageCourseModulesView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly Course _course;

        public ManageCourseModulesView(EduTrackContext context, AuthService authService, Course course)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _course = course;
            
            LoadModules();
        }

        private async void LoadModules()
        {
            var modules = await _context.Modules
                .Include(m => m.Lessons)
                .Where(m => m.CourseId == _course.Id)
                .OrderBy(m => m.Order)
                .ToListAsync();
            
            ModulesItemsControl.ItemsSource = modules;
        }

        private async void OnAddModuleClick(object? sender, RoutedEventArgs e)
        {
            var module = new Module
            {
                Title = "Новый модуль",
                Description = "",
                Order = await _context.Modules.Where(m => m.CourseId == _course.Id).CountAsync() + 1,
                CourseId = _course.Id
            };
            _context.Modules.Add(module);
            await _context.SaveChangesAsync();
            LoadModules();
        }

        private void OnAddLessonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Module module)
            {
                var window = new CreateLessonView(_context, _authService, _course.Id);
                window.Show();
                window.Closed += (s, args) => LoadModules();
            }
        }

        private async void OnEditModuleClick(object? sender, RoutedEventArgs e)
        {
            // Упрощенная версия - можно расширить
            if (sender is Button button && button.CommandParameter is Module module)
            {
                LoadModules();
            }
        }

        private async void OnDeleteModuleClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Module module)
            {
                _context.Modules.Remove(module);
                await _context.SaveChangesAsync();
                LoadModules();
            }
        }
    }
}



