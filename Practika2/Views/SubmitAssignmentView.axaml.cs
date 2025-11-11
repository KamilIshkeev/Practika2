using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class SubmitAssignmentView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly Assignment _assignment;

        public SubmitAssignmentView(EduTrackContext context, AuthService authService, Assignment assignment)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _assignment = assignment;
            
            TitleTextBlock.Text = assignment.Title;
            DescriptionTextBlock.Text = assignment.Description;
        }

        private async void OnSubmitClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            var submission = new AssignmentSubmission
            {
                AssignmentId = _assignment.Id,
                StudentId = _authService.CurrentUser.Id,
                Content = ContentTextBox.Text ?? "",
                SubmittedAt = DateTime.UtcNow
            };
            
            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            
            this.Close();
        }
    }
}



