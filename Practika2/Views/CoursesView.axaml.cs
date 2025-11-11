using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CoursesView : UserControl
    {
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly CourseService _courseService;
        private readonly EnrollmentService _enrollmentService;
        private List<Course> _allCourses = new();

        public CoursesView(EduTrackContext context, AuthService authService, 
                          CourseService courseService, EnrollmentService enrollmentService)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseService = courseService;
            _enrollmentService = enrollmentService;
            
            CategoryFilterComboBox.SelectionChanged += OnFilterChanged;
            DifficultyFilterComboBox.SelectionChanged += OnFilterChanged;
        }

        public void LoadCourses(List<Course> courses)
        {
            _allCourses = courses;
            ApplyFilters();
        }

        private void OnFilterChanged(object? sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allCourses.AsEnumerable();
            
            // Фильтр по категории
            if (CategoryFilterComboBox.SelectedIndex > 0)
            {
                var category = (CourseCategory)(CategoryFilterComboBox.SelectedIndex - 1);
                filtered = filtered.Where(c => c.Category == category);
            }
            
            // Фильтр по сложности
            if (DifficultyFilterComboBox.SelectedIndex > 0)
            {
                var difficulty = (DifficultyLevel)(DifficultyFilterComboBox.SelectedIndex - 1);
                filtered = filtered.Where(c => c.Difficulty == difficulty);
            }
            
            var filteredList = filtered.ToList();
            CoursesItemsControl.ItemsSource = filteredList;
            NoCoursesTextBlock.IsVisible = !filteredList.Any();
        }

        private void OnCourseDetailsClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Course course)
            {
                var detailsWindow = new CourseDetailsView(_context, _authService, 
                    _courseService, _enrollmentService, course);
                detailsWindow.Show();
            }
        }
    }
}
