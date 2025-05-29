using Etalem.Data;
using Etalem.Models;
using Etalem.Models.DTOs;
using Etalem.Models.DTOs.Course;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class QuizAttemptService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizAttemptService> _logger;

        public QuizAttemptService(ApplicationDbContext context, ILogger<QuizAttemptService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<QuizAttemptDto> StartAttemptAsync(int quizId, string studentId)
        {
            _logger.LogInformation("Starting attempt for quiz ID: {QuizId} by student: {StudentId}", quizId, studentId);

            var quiz = await _context.Quizzes
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                throw new Exception("Quiz not found.");
            }

            var attemptsCount = quiz.Attempts?.Count(a => a.StudentId == studentId) ?? 0;
            if (attemptsCount >= quiz.MaxAttempts)
            {
                _logger.LogWarning("Student {StudentId} has exceeded max attempts for quiz {QuizId}", studentId, quizId);
                throw new Exception("You have exceeded the maximum number of attempts for this quiz.");
            }

            var attempt = new QuizAttempt
            {
                StudentId = studentId,
                QuizId = quizId,
                StartedAt = DateTime.UtcNow,
                AttemptNumber = attemptsCount + 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return new QuizAttemptDto
            {
                Id = attempt.Id,
                StudentId = studentId,
                QuizId = quizId,
                StartedAt = attempt.StartedAt,
                AttemptNumber = attempt.AttemptNumber
            };
        }

        public async Task<QuizAttemptDto> SubmitAttemptAsync(int attemptId, List<(int QuestionId, string SelectedAnswer)> answers)
        {
            _logger.LogInformation("Submitting attempt ID: {AttemptId}", attemptId);

            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
            {
                _logger.LogWarning("Attempt not found: {AttemptId}", attemptId);
                throw new Exception("Attempt not found.");
            }

            var totalScore = 0;
            var attemptAnswers = new List<Answer>();

            foreach (var answer in answers)
            {
                var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question == null) continue;

                var isCorrect = answer.SelectedAnswer == question.CorrectAnswer;
                var pointsEarned = isCorrect ? question.Points : 0;

                totalScore += pointsEarned;

                attemptAnswers.Add(new Answer
                {
                    QuizAttemptId = attemptId,
                    QuestionId = answer.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Answers.AddRange(attemptAnswers);
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.Score = totalScore;
            attempt.IsPassed = totalScore >= attempt.Quiz.PassingScore;
            await _context.SaveChangesAsync();

            
            var answerDtos = attemptAnswers.Select(a =>
            {
                var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == a.QuestionId);
                return new AnswerDto
                {
                    Id = a.Id,
                    QuizAttemptId = a.QuizAttemptId,
                    QuestionId = a.QuestionId,
                    SelectedAnswer = a.SelectedAnswer,
                    IsCorrect = a.IsCorrect,
                    PointsEarned = a.PointsEarned,
                    CorrectAnswer = question?.CorrectAnswer
                };
            }).ToList();

            return new QuizAttemptDto
            {
                Id = attempt.Id,
                StudentId = attempt.StudentId,
                QuizId = attempt.QuizId,
                StartedAt = attempt.StartedAt,
                CompletedAt = attempt.CompletedAt,
                Score = attempt.Score,
                IsPassed = attempt.IsPassed,
                AttemptNumber = attempt.AttemptNumber,
                Answers = answerDtos
            };
        }
    }
}