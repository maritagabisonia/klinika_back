using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace hospital.service
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions);
        void DeleteFile(string fileNameWithExtension);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedFileExtensions)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile));
            }

            var contentPath = _environment.WebRootPath; // Use WebRootPath to align with static file serving
            var path = Path.Combine(contentPath, "Uploads");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedFileExtensions.Contains(ext))
            {
                throw new InvalidOperationException("Invalid file extension.");
            }

            var fileName = Guid.NewGuid().ToString() + ext;
            var fileNameWithPath = Path.Combine(path, fileName);

            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteFile(string fileNameWithExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithExtension))
            {
                throw new ArgumentNullException(nameof(fileNameWithExtension));
            }

            var contentPath = _environment.WebRootPath;
            var path = Path.Combine(contentPath, "Uploads", fileNameWithExtension);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new FileNotFoundException("File not found.", fileNameWithExtension);
            }
        }
    }
}
