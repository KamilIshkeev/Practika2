using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class RegisterView : Window
    {
        private readonly AuthService _authService;
        private readonly EduTrackContext _context;

        public RegisterView(EduTrackContext context, AuthService authService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            
            RegisterButton.Click += OnRegisterClick;
        }

        private async void OnRegisterClick(object? sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text ?? "";
            var email = EmailTextBox.Text ?? "";
            var password = PasswordTextBox.Text ?? "";
            var roleIndex = RoleComboBox.SelectedIndex;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text = "Заполните все поля";
                ErrorTextBlock.IsVisible = true;
                return;
            }

            var role = roleIndex switch
            {
                0 => UserRole.Administrator,
                1 => UserRole.Teacher,
                _ => UserRole.Student
            };

            var success = await _authService.RegisterAsync(username, email, password, role);
            
            if (success)
            {
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Пользователь с таким именем или email уже существует";
                ErrorTextBlock.IsVisible = true;
            }
        }
    }
}







