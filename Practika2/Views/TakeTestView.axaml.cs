using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class TakeTestView : Window
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;
        private readonly Test _test;
        private readonly CourseEnrollment _enrollment;
        private readonly Dictionary<int, List<int>> _answers = new();

        public TakeTestView(Func<EduTrackContext> contextFactory, AuthService authService, Test test, CourseEnrollment enrollment)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;
            _test = test;
            _enrollment = enrollment;
            
            LoadTest();
        }

        private void LoadTest()
        {
            TestTitleTextBlock.Text = _test.Title;
            TestDescriptionTextBlock.Text = _test.Description;
            
            foreach (var question in _test.Questions.OrderBy(q => q.Order))
            {
                var questionPanel = new StackPanel { Margin = new Avalonia.Thickness(0, 0, 0, 16) };
                
                var questionText = new TextBlock 
                { 
                    Text = question.Text,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                };
                questionPanel.Children.Add(questionText);
                
                var selectedOptions = new List<int>();
                
                foreach (var option in question.Options.OrderBy(o => o.Id))
                {
                    var checkBox = new CheckBox 
                    { 
                        Content = option.Text,
                        Tag = option.Id
                    };
                    checkBox.IsCheckedChanged += (s, e) =>
                    {
                        var optId = checkBox.Tag as int?;
                        if (optId.HasValue)
                        {
                            if (checkBox.IsChecked == true)
                            {
                                if (!selectedOptions.Contains(optId.Value))
                                    selectedOptions.Add(optId.Value);
                            }
                            else if (checkBox.IsChecked == false)
                            {
                                selectedOptions.Remove(optId.Value);
                            }
                        }
                        _answers[question.Id] = selectedOptions.ToList();
                    };
                    questionPanel.Children.Add(checkBox);
                }
                
                TestPanel.Children.Add(questionPanel);
            }
            
            var submitButton = new Button 
            { 
                Content = "Отправить ответы",
            };
            submitButton.Classes.Add("primary");
            submitButton.Click += OnSubmitClick;
            TestPanel.Children.Add(submitButton);
        }

        private async void OnSubmitClick(object? sender, RoutedEventArgs e)
        {
            if (_authService.CurrentUser == null) return;
            
            using var context = _contextFactory();
            var testService = new TestService(context);

            var submission = await testService.SubmitAsync(_test.Id, _authService.CurrentUser.Id, _answers);
            
            var resultWindow = new Window
            {
                Title = "Результат теста",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(24),
                    Children =
                    {
                        new TextBlock 
                        { 
                            Text = $"Ваш результат: {submission.ScorePercent}%",
                            FontSize = 20,
                            FontWeight = Avalonia.Media.FontWeight.Bold
                        },
                        new TextBlock 
                        { 
                            Text = submission.IsPassed ? "Тест пройден!" : "Тест не пройден",
                            Margin = new Avalonia.Thickness(0, 8, 0, 0)
                        }
                    }
                }
            };
            resultWindow.Show();
            this.Close();
        }
    }
}