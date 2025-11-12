using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class CreateTestView : Window
    {
        private readonly Func<EduTrackContext> _contextFactory;
        private readonly AuthService _authService;
        private readonly int _courseId;
        private readonly Test? _existingTest;
        private readonly ObservableCollection<TestQuestion> _questions = new();

        public CreateTestView(Func<EduTrackContext> contextFactory, AuthService authService, int courseId, Test? existingTest = null)
        {
            InitializeComponent();
            _contextFactory = contextFactory;
            _authService = authService;
            _courseId = courseId;
            _existingTest = existingTest;
            
            DataContext = this;
            QuestionsItemsControl.ItemsSource = _questions;

            LoadLessons();
            
            if (_existingTest != null)
            {
                LoadTestData();
            }
        }

        private async void LoadLessons()
        {
            using var context = _contextFactory();
            var lessons = await context.Lessons
                .Include(l => l.Module)
                .Where(l => l.Module.CourseId == _courseId)
                .ToListAsync();
            
            LessonComboBox.ItemsSource = lessons;
            if (_existingTest != null)
            {
                LessonComboBox.SelectedItem = lessons.FirstOrDefault(l => l.Id == _existingTest.LessonId);
            }
        }

        private void LoadTestData()
        {
            if (_existingTest == null) return;
            
            TitleTextBox.Text = _existingTest.Title;
            DescriptionTextBox.Text = _existingTest.Description;
            TimeLimitTextBox.Text = _existingTest.TimeLimitMinutes.ToString();
            PassingScoreTextBox.Text = _existingTest.PassingScorePercent.ToString();

            using var context = _contextFactory();
            var questions = context.TestQuestions
                .Include(q => q.Options)
                .Where(q => q.TestId == _existingTest.Id)
                .ToList();

            foreach (var q in questions)
            {
                _questions.Add(q);
            }
        }

        private void OnAddQuestionClick(object? sender, RoutedEventArgs e)
        {
            var question = new TestQuestion
            {
                Text = "Новый вопрос",
                Type = QuestionType.SingleChoice,
                Options = new ObservableCollection<TestAnswerOption>
                {
                    new TestAnswerOption { Text = "Новый вариант", IsCorrect = true }
                }
            };
            _questions.Add(question);
        }

        private void OnAddOptionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TestQuestion question)
            {
                question.Options.Add(new TestAnswerOption { Text = "Новый вариант" });
            }
        }

        private void OnRemoveOptionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TestAnswerOption option)
            {
                var question = _questions.FirstOrDefault(q => q.Options.Contains(option));
                question?.Options.Remove(option);
            }
        }

        private void OnRemoveQuestionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is TestQuestion question)
            {
                _questions.Remove(question);
            }
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            if (LessonComboBox.SelectedItem is not Lesson lesson) return;
            
            using var context = _contextFactory();
            var testService = new TestService(context);

            var test = _existingTest ?? new Test();
            test.Title = TitleTextBox.Text ?? "";
            test.Description = DescriptionTextBox.Text ?? "";
            test.TimeLimitMinutes = int.TryParse(TimeLimitTextBox.Text, out var timeLimit) ? timeLimit : 30;
            test.PassingScorePercent = int.TryParse(PassingScoreTextBox.Text, out var passingScore) ? passingScore : 60;
            test.LessonId = lesson.Id;
            
            await testService.SaveTestWithQuestionsAsync(test, _questions.ToList());
            
            this.Close();
        }
    }
}