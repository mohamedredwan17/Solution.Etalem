using Etalem.Data;
using Etalem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SelectPdf;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class CertificateGenerationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CertificateGenerationService> _logger;
        private readonly ConcurrentQueue<(int enrollmentId, string studentId)> _generationQueue;
        private readonly ConcurrentDictionary<int, (bool isCompleted, string certificateUrl, string certificateNumber)> _generationStatus;

        public CertificateGenerationService(IServiceProvider serviceProvider, ILogger<CertificateGenerationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _generationQueue = new ConcurrentQueue<(int, string)>();
            _generationStatus = new ConcurrentDictionary<int, (bool, string, string)>();
        }

        public void QueueCertificateGeneration(int enrollmentId, string studentId)
        {
            _generationQueue.Enqueue((enrollmentId, studentId));
            _generationStatus.TryAdd(enrollmentId, (false, null, null));
            _logger.LogInformation("Queued certificate generation for enrollment ID: {EnrollmentId}", enrollmentId);
        }

        public (bool isCompleted, string certificateUrl, string certificateNumber) GetGenerationStatus(int enrollmentId)
        {
            if (_generationStatus.TryGetValue(enrollmentId, out var status))
            {
                return status;
            }
            return (false, null, null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_generationQueue.TryDequeue(out var task))
                {
                    var (enrollmentId, studentId) = task;
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CertificateGenerationService>>();

                            var enrollment = await context.Enrollments
                                .Include(e => e.Course)
                                .Include(e => e.Student)
                                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId, stoppingToken);

                            if (enrollment == null)
                            {
                                logger.LogWarning("Enrollment not found: {EnrollmentId}", enrollmentId);
                                _generationStatus[enrollmentId] = (true, null, null);
                                continue;
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
            margin-top: 500px; /* زيادة المسافة من فوق */
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

                            
                            var converter = new HtmlToPdf();
                            converter.Options.PdfPageSize = PdfPageSize.A4;
                            converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
                            converter.Options.MarginLeft = 20;
                            converter.Options.MarginRight = 20;
                            converter.Options.MarginTop = 30; 
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
                            await context.SaveChangesAsync(stoppingToken);

                            string finalCertificatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", certificateUrl.TrimStart('/'));
                            if (!File.Exists(finalCertificatePath))
                            {
                                logger.LogError("Certificate file not found after generation at: {FinalCertificatePath}", finalCertificatePath);
                                _generationStatus[enrollmentId] = (true, null, null);
                                continue;
                            }

                            _generationStatus[enrollmentId] = (true, certificateUrl, certificateNumber);
                            logger.LogInformation("Certificate generated successfully for enrollment ID: {EnrollmentId}", enrollmentId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error generating certificate for enrollment ID: {EnrollmentId}. Error: {Error}", enrollmentId, ex.Message);
                        _generationStatus[enrollmentId] = (true, null, null);
                    }
                }

                await Task.Delay(100, stoppingToken); 
            }
        }
    }
}