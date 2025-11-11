using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;
using Practika2.Views;

namespace Practika2
{
    public partial class MainWindow : Window
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;

        public MainWindow(EduTrackContext context, AuthService authService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseService = new CourseService(_context);
            _enrollmentService = new EnrollmentService(_context);
            
            InitializeWindow();
            SetupEvents();
            LoadCourses();
        }

        private void InitializeWindow()
        {
            if (_authService.CurrentUser != null)
            {
                UserNameTextBlock.Text = $"{_authService.CurrentUser.FirstName} {_authService.CurrentUser.LastName} ({_authService.CurrentUser.Role})";
                
                // Показать панель администратора только для администраторов
                AdminPanelButton.IsVisible = _authService.IsAdmin;
                // Показать панель преподавателя только для преподавателей
                TeacherPanelButton.IsVisible = _authService.IsTeacher;
            }
        }

        private void SetupEvents()
        {
            LogoutButton.Click += OnLogoutClick;
            CoursesButton.Click += (s, e) => LoadCourses();
            MyCoursesButton.Click += (s, e) => LoadMyCourses();
            AssignmentsButton.Click += (s, e) => LoadAssignments();
            AdminPanelButton.Click += (s, e) => LoadAdminPanel();
            TeacherPanelButton.Click += (s, e) => LoadTeacherPanel();
        }

        private async void LoadCourses()
        {
            var courses = await _courseService.GetCoursesAsync(publishedOnly: true);
            var coursesView = new CoursesView(_context, _authService, _courseService, _enrollmentService);
            coursesView.LoadCourses(courses);
            ContentArea.Content = coursesView;
        }

        private async void LoadMyCourses()
        {
            if (_authService.CurrentUser == null) return;
            
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(_authService.CurrentUser.Id);
            var myCoursesView = new MyCoursesView(_context, _authService, _courseService, _enrollmentService);
            myCoursesView.LoadEnrollments(enrollments);
            ContentArea.Content = myCoursesView;
        }

        private void LoadAssignments()
        {
            var assignmentsView = new AssignmentsView(_context, _authService);
            ContentArea.Content = assignmentsView;
        }

        private void LoadAdminPanel()
        {
            if (!_authService.IsAdmin) return;
            
            var adminView = new AdminPanelView(_context, _authService, _courseService);
            ContentArea.Content = adminView;
        }

        private void LoadTeacherPanel()
        {
            if (!_authService.IsTeacher) return;
            
            var teacherView = new TeacherPanelView(_context, _authService);
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