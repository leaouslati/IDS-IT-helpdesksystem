using backend.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace backend.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _rootPath;
        private readonly string _wwwrootPath;

        private static readonly Dictionary<string, string> MimeTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg",  "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png",  "image/png" },
                { ".gif",  "image/gif" },
                { ".webp", "image/webp" },
                { ".pdf",  "application/pdf" },
                { ".doc",  "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls",  "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".ppt",  "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".txt",  "text/plain" },
                { ".csv",  "text/csv" },
                { ".log",  "text/plain" },
            };

        public FileStorageService(IWebHostEnvironment env)
        {
            _rootPath    = Path.Combine(env.ContentRootPath, "App_Data", "uploads", "tickets");
            _wwwrootPath = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<(string storedFileName, string virtualPath)> SaveFileAsync(
            Stream stream, int ticketId, string originalFileName, string extension)
        {
            var dir = Path.Combine(_rootPath, ticketId.ToString());
            Directory.CreateDirectory(dir);

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var physicalPath   = Path.Combine(dir, storedFileName);

            await using var fs = new FileStream(physicalPath, FileMode.Create);
            await stream.CopyToAsync(fs);

            var virtualPath = $"/uploads/tickets/{ticketId}/{storedFileName}";
            return (storedFileName, virtualPath);
        }

        public async Task<(Stream? stream, string contentType)> GetFileStreamAsync(
            string storedFileName, int ticketId, string? legacyFilePath = null)
        {
            if (!string.IsNullOrEmpty(storedFileName))
            {
                var path = Path.Combine(_rootPath, ticketId.ToString(), storedFileName);
                if (!File.Exists(path)) return (null, string.Empty);

                var ext         = Path.GetExtension(storedFileName);
                var contentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");
                return (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), contentType);
            }

            if (!string.IsNullOrEmpty(legacyFilePath))
            {
                // Legacy files are in wwwroot/uploads/attachments/{ticketId}/
                var relativePart = legacyFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var path         = Path.Combine(_wwwrootPath, relativePart);
                if (!File.Exists(path)) return (null, string.Empty);

                var ext         = Path.GetExtension(legacyFilePath);
                var contentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");
                return (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), contentType);
            }

            return (null, string.Empty);
        }

        public void DeleteFile(string storedFileName, int ticketId)
        {
            if (string.IsNullOrEmpty(storedFileName)) return;
            var path = Path.Combine(_rootPath, ticketId.ToString(), storedFileName);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
