using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
using System;

namespace Practika2.Views
{
    public partial class CreateUserView : Window
    {
        private readonly EduTrackContext _context;
        private readonly UserManagementService _userManagementService;
        private readonly User? _user;

        public CreateUserView(EduTrackContext context, User? user = null)
        {
            InitializeComponent();
            _context = context;
            _userManagementService = new UserManagementService(_context);
            _user = user;

            if (_user != null)
            {
                this.FindControl<TextBox>("UsernameTextBox")!.Text = _user.Username;
                this.FindControl<TextBox>("EmailTextBox")!.Text = _user.Email;
                this.FindControl<TextBox>("FirstNameTextBox")!.Text = _user.FirstName;
                this.FindControl<TextBox>("LastNameTextBox")!.Text = _user.LastName;
                this.FindControl<ComboBox>("RoleComboBox")!.SelectedIndex = (int)_user.Role;
            }

            this.FindControl<Button>("SaveButton")!.Click += OnSaveButtonClick;
        }

        private async void OnSaveButtonClick(object? sender, RoutedEventArgs e)
        {
            var username = this.FindControl<TextBox>("UsernameTextBox")!.Text;
            var email = this.FindControl<TextBox>("EmailTextBox")!.Text;
            var password = this.FindControl<TextBox>("PasswordTextBox")!.Text;
            var firstName = this.FindControl<TextBox>("FirstNameTextBox")!.Text;
            var lastName = this.FindControl<TextBox>("LastNameTextBox")!.Text;
            var role = (UserRole)this.FindControl<ComboBox>("RoleComboBox")!.SelectedIndex;

            if (_user == null)
            {
                // Create new user
                await _userManagementService.CreateUserAsync(username, email, password, firstName, lastName, role);
            }
            else
            {
                // Update existing user
                _user.Username = username;
                _user.Email = email;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    _user.PasswordHash = password;
                }
                _user.FirstName = firstName;
                _user.LastName = lastName;
                _user.Role = role;
                await _userManagementService.UpdateUserAsync(_user);
            }

            this.Close();
        }
    }
}
