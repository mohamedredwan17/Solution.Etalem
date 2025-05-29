using AutoMapper;
using Etalem.Data;
using Etalem.Models;
using Etalem.Models.DTOs;
using Etalem.Models.DTOs.Course;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class EnrollmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnrollmentService(ApplicationDbContext context, IMapper mapper, ILogger<EnrollmentService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> EnrollAsync(int courseId)
        {
            _logger.LogInformation("Attempting to enroll student for course ID: {CourseId}", courseId);

            var studentId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogWarning("No student ID found in current user context for course: {CourseId}", courseId);
                throw new Exception("Student ID is required.");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found: {CourseId}", courseId);
                throw new Exception("Course not found.");
            }

            if (await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId))
            {
                _logger.LogWarning("Student {StudentId} is already enrolled in course {CourseId}", studentId, courseId);
                throw new Exception("Already enrolled in this course.");
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrollmentDate = DateTime.UtcNow
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            course.EnrollmentCount++;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student {StudentId} enrolled successfully in course {CourseId}", studentId, courseId);
            return enrollment.Id;
        }

        public async Task<EnrollmentDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving enrollment with ID: {EnrollmentId}", id);
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found: {EnrollmentId}", id);
                throw new Exception("Enrollment not found.");
            }

            return _mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudentAsync()
        {
            _logger.LogInformation("Retrieving enrollments for current student");
            var studentId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
            {
                _logger.LogWarning("No student ID found in current user context");
                return new List<EnrollmentDto>();
            }

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Quizzes)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
        }

        public async Task<CourseDto> GetCourseContentAsync(int courseId, string studentId)
        {
            _logger.LogInformation("Retrieving content for course ID: {CourseId} for student: {StudentId}", courseId, studentId);

            var enrollment = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.CourseId == courseId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                        .ThenInclude(l => l.Resources)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Quizzes)
                .FirstOrDefaultAsync();

            if (enrollment == null)
            {
                _logger.LogWarning("Student {StudentId} is not enrolled in course {CourseId}", studentId, courseId);
                throw new Exception("You are not enrolled in this course.");
            }

            var course = enrollment.Course;

            // Map Lessons to CourseContentItem
            var lessonItems = course.Lessons?.Select(l => new CourseContentItem
            {
                Id = l.Id,
                Title = l.Title,
                Order = l.Order,
                Type = "Lesson",
                Duration = l.Duration,
                Resources = l.Resources?.Select(r => new LessonResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    ResourceUrl = r.ResourceUrl
                }).ToList() ?? new List<LessonResourceDto>()
            }).ToList() ?? new List<CourseContentItem>();

            // Map Quizzes to CourseContentItem
            var quizItems = course.Quizzes?.Select(q => new CourseContentItem
            {
                Id = q.Id,
                Title = q.Title,
                Order = q.Order,
                Type = "Quiz",
                TimeLimit = q.TimeLimit,
                PassingScore = q.PassingScore,
                MaxAttempts = q.MaxAttempts
            }).ToList() ?? new List<CourseContentItem>();

            // Combine and sort by Order
            var contentItems = lessonItems.Concat(quizItems)
                .OrderBy(item => item.Order)
                .ToList();

            // Map course to CourseDto and set the combined content items
            var courseDto = _mapper.Map<CourseDto>(course);
            courseDto.ContentItems = contentItems;

            // Mark completed lessons
            var completedLessonIds = await _context.CompletedLessons
                .Where(cl => cl.StudentId == studentId && cl.Lesson.CourseId == courseId)
                .Select(cl => cl.LessonId)
                .ToListAsync();

            // Check quiz attempts to mark if passed
            foreach (var item in courseDto.ContentItems)
            {
                if (item.Type == "Lesson" && completedLessonIds.Contains(item.Id))
                {
                    item.IsCompleted = true;
                }
                else if (item.Type == "Quiz")
                {
                    // محاولات الكويز
                    var quizAttempts = await _context.QuizAttempts
                        .Where(a => a.QuizId == item.Id && a.StudentId == studentId)
                        .ToListAsync();

                    // إذا فيه أي محاولة ناجحة، نحدد الكويز كـ Passed
                    item.IsPassed = quizAttempts.Any(a => a.IsPassed);
                }
            }

            return courseDto;
        }

        public async Task<LessonDto> GetLessonDetailsAsync(int lessonId, string studentId)
        {
            _logger.LogInformation("Retrieving details for lesson ID: {LessonId} for student: {StudentId}", lessonId, studentId);

            var lesson = await _context.Lessons
                .Where(l => l.Id == lessonId)
                .Include(l => l.Course)
                .Include(l => l.Resources)
                .Include(l => l.Discussions)
                .ThenInclude(d => d.User)
                .Include(l => l.Discussions)
                .ThenInclude(d => d.Replies)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found: {LessonId}", lessonId);
                throw new Exception("Lesson not found.");
            }

            var enrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == lesson.CourseId);

            if (!enrollment)
            {
                _logger.LogWarning("Student {StudentId} is not enrolled in course containing lesson {LessonId}", studentId, lessonId);
                throw new Exception("You are not enrolled in this course.");
            }

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task MarkLessonAsCompletedAsync(int lessonId, string studentId)
        {
            _logger.LogInformation("Marking lesson ID: {LessonId} as completed for student: {StudentId}", lessonId, studentId);

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                _logger.LogWarning("Lesson not found: {LessonId}", lessonId);
                throw new Exception("Lesson not found.");
            }

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == lesson.CourseId);

            if (enrollment == null)
            {
                _logger.LogWarning("Student {StudentId} is not enrolled in course containing lesson {LessonId}", studentId, lessonId);
                throw new Exception("You are not enrolled in this course.");
            }

            var completedLesson = await _context.CompletedLessons
                .FirstOrDefaultAsync(cl => cl.StudentId == studentId && cl.LessonId == lessonId);

            if (completedLesson == null)
            {
                completedLesson = new CompletedLesson
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    CompletedAt = DateTime.UtcNow
                };
                await _context.CompletedLessons.AddAsync(completedLesson);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Lesson {LessonId} marked as completed for student {StudentId}", lessonId, studentId);
            }

            if (lesson.CourseId.HasValue)
            {
                await CalculateAndUpdateProgressAsync(enrollment.Id, lesson.CourseId.Value, studentId);
            }
            else
            {
                _logger.LogWarning("CourseId is null for lesson ID: {LessonId}", lessonId);
                throw new Exception("Invalid course association for the lesson.");
            }
        }

        public async Task CalculateAndUpdateProgressAsync(int enrollmentId, int courseId, string studentId)
        {
            _logger.LogInformation("Calculating progress for enrollment ID: {EnrollmentId}", enrollmentId);

            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == courseId);

            if (totalLessons == 0)
            {
                _logger.LogWarning("No lessons found for course ID: {CourseId}", courseId);
                return;
            }

            var completedLessons = await _context.CompletedLessons
                .CountAsync(cl => cl.StudentId == studentId && cl.Lesson.CourseId == courseId);

            var progress = (int)((completedLessons / (double)totalLessons) * 100);
            _logger.LogInformation("Progress for enrollment {EnrollmentId}: {CompletedLessons}/{TotalLessons} = {Progress}%", enrollmentId, completedLessons, totalLessons, progress);

            await UpdateProgressAsync(enrollmentId, progress);
        }

        public async Task<QuizDto> GetQuizDetailsAsync(int quizId, string studentId)
        {
            _logger.LogInformation("Retrieving details for quiz ID: {QuizId} for student: {StudentId}", quizId, studentId);

            var quiz = await _context.Quizzes
                .Where(q => q.Id == quizId)
                .AsSplitQuery()
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                throw new Exception("Quiz not found.");
            }

            var enrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == quiz.CourseId);

            if (!enrollment)
            {
                _logger.LogWarning("Student {StudentId} is not enrolled in course containing quiz {QuizId}", studentId, quizId);
                throw new Exception("You are not enrolled in this course.");
            }

            return _mapper.Map<QuizDto>(quiz);
        }

        public async Task<QuizAttemptDto> SubmitQuizAttemptAsync(int quizId, string studentId, List<(int QuestionId, string SelectedAnswer)> answers)
        {
            _logger.LogInformation("Submitting quiz attempt for quiz ID: {QuizId} by student: {StudentId}", quizId, studentId);

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                _logger.LogWarning("Quiz not found: {QuizId}", quizId);
                throw new Exception("Quiz not found.");
            }

            var enrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == quiz.CourseId);

            if (!enrollment)
            {
                _logger.LogWarning("Student {StudentId} is not enrolled in course containing quiz {QuizId}", studentId, quizId);
                throw new Exception("You are not enrolled in this course.");
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

            await _context.QuizAttempts.AddAsync(attempt);

            var totalScore = 0;
            var attemptAnswers = new List<Answer>();

            foreach (var answer in answers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question == null)
                {
                    _logger.LogWarning("Question {QuestionId} not found in quiz {QuizId}", answer.QuestionId, quizId);
                    continue;
                }

                var isCorrect = answer.SelectedAnswer == question.CorrectAnswer;
                var pointsEarned = isCorrect ? question.Points : 0;

                totalScore += pointsEarned;

                attemptAnswers.Add(new Answer
                {
                    QuizAttempt = attempt,
                    QuestionId = answer.QuestionId,
                    SelectedAnswer = answer.SelectedAnswer,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.Answers.AddRangeAsync(attemptAnswers);

            attempt.CompletedAt = DateTime.UtcNow;
            attempt.Score = totalScore;
            attempt.IsPassed = totalScore >= quiz.PassingScore;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Quiz attempt submitted successfully for quiz ID: {QuizId} by student: {StudentId}", quizId, studentId);
            return _mapper.Map<QuizAttemptDto>(attempt);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourseAsync(int courseId)
        {
            _logger.LogInformation("Retrieving enrollments for course ID: {CourseId}", courseId);
            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
        }

        public async Task UpdateProgressAsync(int enrollmentId, int progress)
        {
            _logger.LogInformation("Updating progress for enrollment ID: {EnrollmentId} to {Progress}", enrollmentId, progress);
            if (progress < 0 || progress > 100)
            {
                throw new ArgumentException("Progress must be between 0 and 100.");
            }

            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found: {EnrollmentId}", enrollmentId);
                throw new Exception("Enrollment not found.");
            }

            enrollment.Progress = progress;
            if (progress == 100 && !enrollment.IsCompleted)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Progress updated successfully for enrollment ID: {EnrollmentId}", enrollmentId);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting enrollment with ID: {EnrollmentId}", id);
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found: {EnrollmentId}", id);
                throw new Exception("Enrollment not found.");
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Enrollment deleted successfully: {EnrollmentId}", id);
        }

        public async Task<bool> CanGenerateCertificateAsync(int enrollmentId, string studentId)
        {
            _logger.LogInformation("Checking if certificate can be generated for enrollment ID: {EnrollmentId}", enrollmentId);

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Quizzes)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found: {EnrollmentId}", enrollmentId);
                throw new Exception("Enrollment not found.");
            }

            if (enrollment.Progress != 100 || !enrollment.IsCompleted)
            {
                _logger.LogInformation("Cannot generate certificate: Course not fully completed for enrollment ID: {EnrollmentId}", enrollmentId);
                return false;
            }

            var quizzes = enrollment.Course.Quizzes;
            if (quizzes != null && quizzes.Any())
            {
                foreach (var quiz in quizzes)
                {
                    var quizAttempts = await _context.QuizAttempts
                        .Where(a => a.QuizId == quiz.Id && a.StudentId == studentId)
                        .ToListAsync();

                    if (!quizAttempts.Any(a => a.IsPassed))
                    {
                        _logger.LogInformation("Cannot generate certificate: Quiz {QuizId} not passed for enrollment ID: {EnrollmentId}", quiz.Id, enrollmentId);
                        return false;
                    }
                }
            }

            _logger.LogInformation("All conditions met for generating certificate for enrollment ID: {EnrollmentId}", enrollmentId);
            return true;
        }

        public async Task<string> GenerateCertificateAsync(int enrollmentId, string studentId)
        {
            _logger.LogInformation("Generating certificate for enrollment ID: {EnrollmentId}", enrollmentId);

            if (!await CanGenerateCertificateAsync(enrollmentId, studentId))
            {
                throw new Exception("Cannot generate certificate: Course requirements not met.");
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

            if (enrollment == null)
            {
                _logger.LogWarning("Enrollment not found: {EnrollmentId}", enrollmentId);
                throw new Exception("Enrollment not found.");
            }

            if (!string.IsNullOrEmpty(enrollment.CertificateUrl))
            {
                string certificatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", enrollment.CertificateUrl.TrimStart('/'));
                if (File.Exists(certificatePath))
                {
                    return enrollment.CertificateUrl;
                }
                else
                {
                    _logger.LogWarning("Certificate file not found at: {CertificatePath}. Regenerating certificate for enrollment ID: {EnrollmentId}", certificatePath, enrollmentId);
                    enrollment.CertificateUrl = null;
                    await _context.SaveChangesAsync();
                }
            }

            string studentName = enrollment.Student?.UserName ?? "Student";
            string courseTitle = enrollment.Course.Title;
            string completionDate = enrollment.CompletedAt?.ToString("MMMM dd, yyyy") ?? DateTime.UtcNow.ToString("MMMM dd, yyyy");

            
            string pdfFileName = $"{enrollmentId}_{Guid.NewGuid()}.pdf".ToLower();
            string certificateNumber = Path.GetFileNameWithoutExtension(pdfFileName); 

           
            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <link href='https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&family=Playfair+Display:wght@700&display=swap' rel='stylesheet'>
    <style>
        body {{
            font-family: 'Roboto', sans-serif;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin-top: 500px;
        }}
        .certificate-container {{
            width: 800px;
            background-color: white;
            border: 10px solid #1a237e;
            border-radius: 15px;
            box-shadow: 0 0 20px rgba(0, 0, 0, 0.2);
            padding: 40px;
            text-align: center;
            position: relative;
        }}
        .certificate-header {{
            font-family: 'Playfair Display', serif;
            font-size: 36px;
            color: #1a237e;
            margin-bottom: 20px;
        }}
        .certificate-body p {{
            font-size: 20px;
            color: #333;
            margin: 10px 0;
        }}
        .certificate-body h2 {{
            font-family: 'Playfair Display', serif;
            font-size: 28px;
            color: #d81b60;
            margin: 10px 0;
        }}
        .certificate-footer {{
            margin-top: 30px;
            font-size: 16px;
            color: #666;
        }}
        .certificate-number {{
            position: absolute;
            top: 20px;
            right: 20px;
            font-size: 14px;
            color: #666;
            font-style: italic;
        }}
        .decorative-line {{
            width: 50%;
            height: 2px;
            background: linear-gradient(to right, #1a237e, #d81b60, #1a237e);
            margin: 20px auto;
        }}
    </style>
</head>
<body>
    <div class='certificate-container'>
        <div class='certificate-number'>Certificate No: {certificateNumber}</div>
        <div class='certificate-header'>Certificate of Completion</div>
        <div class='certificate-body'>
            <p>This is to certify that</p>
            <h2>{studentName}</h2>
            <p>has successfully completed the course</p>
            <h2>{courseTitle}</h2>
            <div class='decorative-line'></div>
            <p>Date of Completion: {completionDate}</p>
        </div>
        <div class='certificate-footer'>
            <p>Issued by Etalem Learning Platform</p>
        </div>
    </div>
</body>
</html>";

            try
            {
                
                var converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
                converter.Options.MarginLeft = 20;
                converter.Options.MarginRight = 20;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 20;

                PdfDocument doc = converter.ConvertHtmlString(htmlContent);

                string certificatesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates");
                if (!Directory.Exists(certificatesDir))
                {
                    Directory.CreateDirectory(certificatesDir);
                }

                string finalPdfPath = Path.Combine(certificatesDir, pdfFileName);

                doc.Save(finalPdfPath);
                doc.Close();

                string certificateUrl = $"/certificates/{pdfFileName}";

                enrollment.CertificateUrl = certificateUrl;
                await _context.SaveChangesAsync();

                string finalCertificatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificateUrl.TrimStart('/'));
                if (!File.Exists(finalCertificatePath))
                {
                    _logger.LogError("Certificate file not found after generation at: {FinalCertificatePath}", finalCertificatePath);
                    throw new Exception("Certificate file not found after generation.");
                }

                _logger.LogInformation("Certificate generated successfully for enrollment ID: {EnrollmentId}", enrollmentId);
                return certificateUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error generating certificate: {Error}", ex.Message);
                throw new Exception("Failed to generate certificate: " + ex.Message, ex);
            }
        }
    }
}