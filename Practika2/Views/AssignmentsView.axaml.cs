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
    public partial class AssignmentsView : UserControl
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;

        public AssignmentsView(EduTrackContext context, AuthService authService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            
            LoadAssignments();
        }

        private async void LoadAssignments()
        {
            if (_authService.CurrentUser == null) return;
            
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.StudentId == _authService.CurrentUser.Id)
                .Select(e => e.CourseId)
                .ToListAsync();
            
            var assignments = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Module)
                        .ThenInclude(m => m.Course)
                .Where(a => enrollments.Contains(a.Lesson.Module.Course.Id))
                .ToListAsync();
            
            AssignmentsItemsControl.ItemsSource = assignments;
            NoAssignmentsTextBlock.IsVisible = assignments.Count == 0;
        }

        private void OnSubmitAssignmentClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Assignment assignment)
            {
                var window = new SubmitAssignmentView(_context, _authService, assignment);
                window.Show();
                window.Closed += (s, args) => LoadAssignments();
            }
        }
    }
}
