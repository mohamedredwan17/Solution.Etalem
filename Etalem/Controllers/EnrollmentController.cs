using Etalem.Models.DTOs;
using Etalem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Etalem.Data;
using Etalem.Models.DTOs.Course;
using SendGrid.Helpers.Mail;
using Stripe;
using System.Security.Policy;
using Microsoft.Extensions.Logging;

namespace Etalem.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly EnrollmentService _enrollmentService;
        private readonly QuizAttemptService _quizAttemptService;
        private readonly ApplicationDbContext _context; 
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(EnrollmentService enrollmentService, QuizAttemptService quizAttemptService, ApplicationDbContext context, IConfiguration configuration, ILogger<EnrollmentController> logger)
        {
            _enrollmentService = enrollmentService;
            _quizAttemptService = quizAttemptService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync();
            return View(enrollments);
        }

        public async Task<IActionResult> CourseContent(int courseId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var course = await _enrollmentService.GetCourseContentAsync(courseId, studentId);
                return View(course);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("Enrollment/LessonDetails/{lessonId}")]
        public async Task<IActionResult> LessonDetails(int lessonId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var lesson = await _enrollmentService.GetLessonDetailsAsync(lessonId, studentId);

                await _enrollmentService.MarkLessonAsCompletedAsync(lessonId, studentId);

                return View(lesson);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> QuizDetails(int quizId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var quiz = await _enrollmentService.GetQuizDetailsAsync(quizId, studentId);

                //  آخر محاولة للطالب
                var attempts = await _context.QuizAttempts
                    .Where(a => a.QuizId == quizId && a.StudentId == studentId)
                    .Include(a => a.Answers) 
                    .OrderByDescending(a => a.AttemptNumber)
                    .ToListAsync();

                var lastAttempt = attempts.FirstOrDefault();
                if (lastAttempt != null)
                {
                    
                    var answerDtos = lastAttempt.Answers.Select(a =>
                    {
                        var question = quiz.Questions.FirstOrDefault(q => q.Id == a.QuestionId);
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

                    ViewBag.LastAttempt = new QuizAttemptDto
                    {
                        Id = lastAttempt.Id,
                        StudentId = lastAttempt.StudentId,
                        QuizId = lastAttempt.QuizId,
                        StartedAt = lastAttempt.StartedAt,
                        CompletedAt = lastAttempt.CompletedAt,
                        Score = lastAttempt.Score,
                        IsPassed = lastAttempt.IsPassed,
                        AttemptNumber = lastAttempt.AttemptNumber,
                        Answers = answerDtos
                    };

                    ViewBag.AttemptsCount = attempts.Count(); 
                }
                else
                {
                    ViewBag.LastAttempt = null;
                    ViewBag.AttemptsCount = 0;
                }

                return View(quiz);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, string> answers)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // بدء محاولة جديدة
                var attempt = await _quizAttemptService.StartAttemptAsync(quizId, studentId);

                
                var answerList = answers.Select(a => (QuestionId: a.Key, SelectedAnswer: a.Value)).ToList();

                
                var submittedAttempt = await _quizAttemptService.SubmitAttemptAsync(attempt.Id, answerList);

                TempData["SuccessMessage"] = $"Quiz submitted successfully! Your score: {submittedAttempt.Score}%. Passed: {(submittedAttempt.IsPassed ? "Yes" : "No")}";
                return RedirectToAction("QuizDetails", new { quizId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("QuizDetails", new { quizId });
            }
        }

        [HttpPost]
        [Route("Enrollment/RetakeQuiz/{quizId}")]
        public async Task<IActionResult> RetakeQuiz(int quizId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // بدء محاولة جديدة
                await _quizAttemptService.StartAttemptAsync(quizId, studentId);

                TempData["SuccessMessage"] = "You can now retake the quiz.";
                return RedirectToAction("QuizDetails", new { quizId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("QuizDetails", new { quizId });
            }
        }

        public async Task<IActionResult> Enroll(int courseId)
        {
            try
            {
                var enrollmentId = await _enrollmentService.EnrollAsync(courseId);
                TempData["SuccessMessage"] = "Enrollment successful!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Course", new { id = courseId });
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _enrollmentService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Enrollment deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Enrollment/GenerateCertificate/{enrollmentId}")]
        public async Task<IActionResult> GenerateCertificate(int enrollmentId)
        {
            try
            {
                var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var certificateUrl = await _enrollmentService.GenerateCertificateAsync(enrollmentId, studentId);
                TempData["SuccessMessage"] = "Certificate generated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(int courseId)
        {
            var studentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["ErrorMessage"] = "Student ID is required.";
                return RedirectToAction("Index");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction("Index");
            }

            // تكوين Stripe
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var baseUrl = Url.Action("CheckoutSuccess", "Enrollment", new { courseId, studentId }, Request.Scheme);
            var successUrl = $"{baseUrl}{(baseUrl.Contains("?") ? "&" : "?")}session_id={{CHECKOUT_SESSION_ID}}";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(course.Price * 100), // Convert dollars to cents
                            Currency = "usd",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = course.Title
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = Url.Action("CheckoutCancel", "Enrollment", new { courseId }, Request.Scheme)
            };

            var service = new Stripe.Checkout.SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        public async Task<IActionResult> CheckoutSuccess(int courseId, string studentId)
        {
            _logger.LogInformation("CheckoutSuccess called with courseId: {CourseId}, studentId: {StudentId}, Query: {Query}", courseId, studentId, Request.QueryString);

            // استخرج sessionId من Query String
            var sessionId = Request.Query["session_id"].ToString();
            _logger.LogInformation("Extracted sessionId from query: {SessionId}", sessionId);

            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.LogWarning("Session ID is null or empty in CheckoutSuccess.");
                TempData["ErrorMessage"] = "Payment verification failed: Invalid session.";
                return RedirectToAction("Index");
            }

            try
            {
                var service = new Stripe.Checkout.SessionService();
                var session = await service.GetAsync(sessionId);

                if (session.PaymentStatus == "paid")
                {
                    var enrollmentId = await _enrollmentService.EnrollAsync(courseId);
                    TempData["SuccessMessage"] = "Payment successful! You are now enrolled.";
                    return RedirectToAction("Index");
                }
                _logger.LogWarning("Payment status is not 'paid' for session: {SessionId}", sessionId);
                TempData["ErrorMessage"] = "Payment verification failed.";
                return RedirectToAction("Index");
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error in CheckoutSuccess for sessionId: {SessionId}", sessionId);
                TempData["ErrorMessage"] = $"Payment verification failed: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        public async Task<IActionResult> CheckoutCancel(int courseId)
        {
            TempData["ErrorMessage"] = "Payment was cancelled.";
            return RedirectToAction("Details", "Course", new { id = courseId });
        }


    }
}