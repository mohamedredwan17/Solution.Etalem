using Etalem.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Etalem.Services
{
    public class FileService : IFileService
    {
        private readonly string _webRootPath;

        public FileService(IWebHostEnvironment environment)
        {
            _webRootPath = environment.WebRootPath;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                return null; // أو يمكن ترجع مسار صورة افتراضية
            }

            // التحقق من نوع الملف
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Only JPG and PNG files are allowed.");
            }

            // التحقق من حجم الملف (مثلاً أقصى 5 ميجا)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxFileSize)
            {
                throw new Exception("File size cannot exceed 5MB.");
            }

            // إنشاء المسار
            var uploadsFolder = Path.Combine(_webRootPath, "Uploads", subfolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // إنشاء اسم ملف فريد
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            var relativePath = $"/Uploads/{subfolder}/{fileName}";

            // حفظ الملف
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return relativePath;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var fullPath = Path.Combine(_webRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }
    }
}