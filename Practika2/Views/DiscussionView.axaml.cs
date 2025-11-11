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
    public partial class DiscussionView : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly DiscussionThread _thread;

        public DiscussionView(EduTrackContext context, AuthService authService, DiscussionThread thread)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _thread = thread;
            
            TitleTextBlock.Text = thread.Title;
            LoadMessages();
        }

        private async void LoadMessages()
        {
            using (var ctx = new EduTrackContext())
            {
                var messages = await ctx.DiscussionMessages
                    .Include(m => m.Author)
                    .Where(m => m.ThreadId == _thread.Id)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
            
                MessagesItemsControl.ItemsSource = messages;
            }
        }

        private async void OnSendClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            using (var ctx = new EduTrackContext())
            {
                var message = new DiscussionMessage
                {
                    ThreadId = _thread.Id,
                    AuthorId = _authService.CurrentUser.Id,
                    Content = MessageTextBox.Text ?? "",
                    CreatedAt = DateTime.UtcNow
                };
                
                ctx.DiscussionMessages.Add(message);
                await ctx.SaveChangesAsync();
            }
            
            MessageTextBox.Text = "";
            LoadMessages();
        }
    }
}


