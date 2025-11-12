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
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly User? _user;

        public CreateUserView(Func<EduTrackContext> contextFactory, User? user = null)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
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

            using var context = _contextFactory();
            var userManagementService = new UserManagementService(context);

            if (_user == null)
            {
                await userManagementService.CreateUserAsync(username, email, password, firstName, lastName, role);
            }
            else
            {
                _user.Username = username;
                _user.Email = email;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    _user.PasswordHash = password;
                }
                _user.FirstName = firstName;
                _user.LastName = lastName;
                _user.Role = role;
                await userManagementService.UpdateUserAsync(_user);
            }

            this.Close();
        }
    }
}