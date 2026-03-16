using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                // إنشاء اسم ملف فريد
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                // إنشاء مسار المجلد
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads", folder);

                // إنشاء المجلد إذا لم يكن موجود
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // المسار الكامل للملف
                var filePath = Path.Combine(folderPath, fileName);

                // حفظ الملف
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // إرجاع الـ URL النسبي
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"File upload error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return false;

                var filePath = Path.Combine(_environment.WebRootPath, fileUrl.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
