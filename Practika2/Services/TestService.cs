using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Practika2.Data;
using Practika2.Models;

namespace Practika2.Services
{
	public class TestService
	{
		private readonly EduTrackContext _context;
		
		public TestService(EduTrackContext context)
		{
			_context = context;
		}
		
		public async Task<Test?> GetTestAsync(int testId)
		{
			return await _context.Tests
				.Include(t => t.Questions)
					.ThenInclude(q => q.Options)
				.FirstOrDefaultAsync(t => t.Id == testId);
		}
		
		public async Task<TestSubmission> SubmitAsync(int testId, int studentId, Dictionary<int, List<int>> answersByQuestion)
		{
			var test = await _context.Tests
				.Include(t => t.Questions)
					.ThenInclude(q => q.Options)
				.FirstOrDefaultAsync(t => t.Id == testId);
			
			if (test == null)
				throw new InvalidOperationException("Test not found");
			
			int totalQuestions = test.Questions.Count;
			int correctCount = 0;
			
			var submission = new TestSubmission
			{
				TestId = test.Id,
				StudentId = studentId,
				SubmittedAt = DateTime.UtcNow
			};
			_context.TestSubmissions.Add(submission);
			await _context.SaveChangesAsync();
			
			foreach (var question in test.Questions.OrderBy(q => q.Order))
			{
				answersByQuestion.TryGetValue(question.Id, out var selectedIds);
				selectedIds ??= new List<int>();
				
				var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).OrderBy(id => id).ToList();
				var selectedSorted = selectedIds.OrderBy(id => id).ToList();
				
				bool isCorrect = correctOptionIds.SequenceEqual(selectedSorted);
				if (isCorrect) correctCount++;
				
				var answer = new TestSubmissionAnswer
				{
					SubmissionId = submission.Id,
					QuestionId = question.Id,
					SelectedOptionId = selectedIds.Count == 1 ? selectedIds[0] : null,
					SelectedOptionIdsCsv = selectedIds.Count > 1 ? string.Join(",", selectedIds) : null
				};
				_context.TestSubmissionAnswers.Add(answer);
			}
			
			await _context.SaveChangesAsync();
			
			int scorePercent = totalQuestions > 0 ? (int)Math.Round((double)correctCount * 100 / totalQuestions) : 0;
			submission.ScorePercent = scorePercent;
			submission.IsPassed = scorePercent >= test.PassingScorePercent;
			
			await _context.SaveChangesAsync();
			return submission;
		}
		
		public async Task<TestStatistics> GetTestStatisticsAsync(int testId)
		{
			var submissions = await _context.TestSubmissions
				.Where(s => s.TestId == testId)
				.ToListAsync();
			
			return new TestStatistics
			{
				TotalSubmissions = submissions.Count,
				AverageScore = submissions.Any() ? submissions.Average(s => s.ScorePercent) : 0,
				PassedCount = submissions.Count(s => s.IsPassed)
			};
		}
	}
}



