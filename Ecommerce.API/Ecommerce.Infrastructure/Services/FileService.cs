using Microsoft.AspNetCore.Hosting;

namespace Ecommerce.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _imagesRootPath; // wwwroot/images

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
            _imagesRootPath = Path.Combine(_env.WebRootPath, FileSettings.ImagesPath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentNullException(nameof(folder));

            var folderPath = Path.Combine(_imagesRootPath, folder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.Combine(FileSettings.ImagesPath, folder, fileName).Replace("\\", "/");
        }

        public bool DeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return false;

            // Combine with wwwroot
            var filePath = Path.Combine(_env.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
    }
}
