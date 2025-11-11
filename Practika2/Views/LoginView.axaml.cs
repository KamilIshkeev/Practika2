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
    public partial class LoginView : Window
    {
        private readonly AuthService _authService;
        private readonly EduTrackContext _context;

        public LoginView()
        {
            InitializeComponent();
            _context = new EduTrackContext();
            _authService = new AuthService(_context);
            
            LoginButton.Click += OnLoginClick;
            RegisterButton.Click += OnRegisterClick;
            
            // Инициализация базы данных и создание администратора по умолчанию
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            try
            {
				// Сначала пытаемся применить миграции (если они есть)
				try
				{
					await _context.Database.MigrateAsync();
				}
				catch
				{
					// Если миграций нет, создаём базу по текущей модели
					await _context.Database.EnsureCreatedAsync();
				}
                
                // Создание администратора по умолчанию, если его нет
                if (!await _context.Users.AnyAsync(u => u.Role == Models.UserRole.Administrator))
                {
                    var admin = new Models.User
                    {
                        Username = "admin",
                        Email = "admin@edutrack.com",
                        // Пароль хранится без хэширования по требованию проекта
                        PasswordHash = "admin123",
                        Role = Models.UserRole.Administrator,
                        FirstName = "Администратор",
                        LastName = "Системы",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(admin);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка инициализации: {ex.Message}";
                ErrorTextBlock.IsVisible = true;
            }
        }

        private async void OnLoginClick(object? sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text ?? "";
            var password = PasswordTextBox.Text ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text = "Введите имя пользователя и пароль";
                ErrorTextBlock.IsVisible = true;
                return;
            }

            var success = await _authService.LoginAsync(username, password);
            
            if (success)
            {
                var mainWindow = new MainWindow(_context, _authService);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Неверное имя пользователя или пароль";
                ErrorTextBlock.IsVisible = true;
            }
        }

        private void OnRegisterClick(object? sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterView(_context, _authService);
            registerWindow.Show();
        }
    }
}
