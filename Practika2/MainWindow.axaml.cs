using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Services;
using Practika2.Views;

namespace Practika2
{
    public partial class MainWindow : Window
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;

        public MainWindow(AuthService authService)
        {
            InitializeComponent();
            _contextFactory = () => new EduTrackContext();
            _authService = authService;
            
            InitializeWindow();
            SetupEvents();
            LoadCourses();
        }

        private void InitializeWindow()
        {
            if (_authService.CurrentUser != null)
            {
                UserNameTextBlock.Text = $"{_authService.CurrentUser.FirstName} {_authService.CurrentUser.LastName} ({_authService.CurrentUser.Role})";
                
                AdminPanelButton.IsVisible = _authService.IsAdmin;
                TeacherPanelButton.IsVisible = _authService.IsTeacher;
            }
        }

        private void SetupEvents()
        {
            LogoutButton.Click += OnLogoutClick;
            CoursesButton.Click += (s, e) => LoadCourses();
            MyCoursesButton.Click += (s, e) => LoadMyCourses();
            AssignmentsButton.Click += (s, e) => LoadAssignments();
            ProfileButton.Click += (s, e) => LoadProfile();
            AdminPanelButton.Click += (s, e) => LoadAdminPanel();
            TeacherPanelButton.Click += (s, e) => LoadTeacherPanel();
        }

        private void LoadProfile()
        {
            var profileView = new ProfileView(_contextFactory, _authService);
            ContentArea.Content = profileView;
        }

        private async void LoadCourses()
        {
            using var context = _contextFactory();
            var courseService = new CourseService(context);
            var courses = await courseService.GetCoursesAsync(publishedOnly: true);
            var coursesView = new CoursesView(_contextFactory, _authService);
            coursesView.LoadCourses(courses);
            ContentArea.Content = coursesView;
        }

        private async void LoadMyCourses()
        {
            if (_authService.CurrentUser == null) return;
            
            using var context = _contextFactory();
            var enrollmentService = new EnrollmentService(context);
            var enrollments = await enrollmentService.GetStudentEnrollmentsAsync(_authService.CurrentUser.Id);

            var myCoursesView = new MyCoursesView(_contextFactory, _authService);
            myCoursesView.LoadEnrollments(enrollments);
            ContentArea.Content = myCoursesView;
        }

        private void LoadAssignments()
        {
            var assignmentsView = new AssignmentsView(_contextFactory, _authService);
            ContentArea.Content = assignmentsView;
        }

        private void LoadAdminPanel()
        {
            if (!_authService.IsAdmin) return;
            
            var adminView = new AdminPanelView(_contextFactory, _authService);
            ContentArea.Content = adminView;
        }

        private void LoadTeacherPanel()
        {
            if (!_authService.IsTeacher) return;
            
            var teacherView = new TeacherPanelView(_contextFactory, _authService);
            ContentArea.Content = teacherView;
        }

        private void OnLogoutClick(object? sender, RoutedEventArgs e)
        {
            _authService.Logout();
            var loginWindow = new LoginView();
            loginWindow.Show();
            this.Close();
        }
    }
}