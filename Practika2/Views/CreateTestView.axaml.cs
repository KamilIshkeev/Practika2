using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
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
        private readonly EduTrackContext _context;
        private readonly AuthService _authService;
        private readonly int _courseId;
        private readonly Test? _existingTest;
        private readonly ObservableCollection<TestQuestion> _questions = new();

        public CreateTestView(EduTrackContext context, AuthService authService, int courseId, Test? existingTest = null)
        {
            InitializeComponent();
            _context = context;
            _authService = authService;
            _courseId = courseId;
            _existingTest = existingTest;
            
            LoadLessons();
            QuestionsItemsControl.ItemsSource = _questions;
            
            if (_existingTest != null)
            {
                LoadTestData();
            }
        }

        private async void LoadLessons()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Module)
                .Where(l => l.Module.CourseId == _courseId)
                .ToListAsync();
            
            LessonComboBox.ItemsSource = lessons;
            LessonComboBox.SelectedItem = _existingTest?.Lesson;
        }

        private void LoadTestData()
        {
            if (_existingTest == null) return;
            
            TitleTextBox.Text = _existingTest.Title;
            DescriptionTextBox.Text = _existingTest.Description;
            TimeLimitTextBox.Text = _existingTest.TimeLimitMinutes.ToString();
            PassingScoreTextBox.Text = _existingTest.PassingScorePercent.ToString();
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
            
            var test = _existingTest ?? new Test();
            test.Title = TitleTextBox.Text ?? "";
            test.Description = DescriptionTextBox.Text ?? "";
            test.TimeLimitMinutes = int.TryParse(TimeLimitTextBox.Text, out var timeLimit) ? timeLimit : 30;
            test.PassingScorePercent = int.TryParse(PassingScoreTextBox.Text, out var passingScore) ? passingScore : 60;
            test.LessonId = lesson.Id;
            
            if (_existingTest == null)
            {
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
            }
            else
            {
                await _context.SaveChangesAsync();
            }
            
            // Добавить вопросы
            foreach (var question in _questions)
            {
                question.TestId = test.Id;
                _context.TestQuestions.Add(question);
                foreach (var option in question.Options)
                {
                    option.QuestionId = question.Id;
                    _context.TestAnswerOptions.Add(option);
                }
            }
            
            await _context.SaveChangesAsync();
            this.Close();
        }
    }
}



