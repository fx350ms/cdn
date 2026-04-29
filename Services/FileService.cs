using FileService.Api.Models;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace FileService.Api.Services
{
    public class FileService
    {
        private readonly IMongoCollection<FileDocument> _files;
        private readonly ILogger<FileService> _logger;

        public FileService(IMongoDatabase database, ILogger<FileService> logger)
        {
            _files = database.GetCollection<FileDocument>("Files");
            _logger = logger;
        }

        public async Task<FileDocument> UploadFileAsync(IFormFile file, string serviceName)
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File cannot be empty");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".zip" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
            }

            // Validate file size (max 20MB)
            const long maxFileSize = 20 * 1024 * 1024; // 20MB
            if (file.Length > maxFileSize)
            {
                throw new ArgumentException("File size exceeds the maximum limit of 20MB");
            }

            // Create directory path
            var date = DateTime.Now;
            var directoryPath = Path.Combine("uploads", serviceName, date.Year.ToString(), date.Month.ToString("00"), date.Day.ToString("00"));
            var directory = Directory.CreateDirectory(directoryPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(directory.FullName, fileName);

            // Save file to disk
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Create document
            var fileDocument = new FileDocument
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                ServiceName = serviceName,
                Extension = extension,
                ContentType = file.ContentType,
                Size = file.Length,
                Path = filePath,
                CreatedAt = DateTime.UtcNow
            };

            // Save metadata to MongoDB
            await _files.InsertOneAsync(fileDocument);

            _logger.LogInformation("File uploaded successfully: {FileName} for service: {ServiceName}", file.FileName, serviceName);

            return fileDocument;
        }

        public async Task<FileDocument> GetFileByIdAsync(string id)
        {
            var filter = Builders<FileDocument>.Filter.Eq(x => x.Id, id);
            var result = await _files.FindAsync(filter);
            return await result.FirstOrDefaultAsync();
        }

        public async Task<(Stream, string, long)> GetFileStreamAsync(string id)
        {
            var fileDocument = await GetFileByIdAsync(id);
            if (fileDocument == null || !File.Exists(fileDocument.Path))
            {
                throw new FileNotFoundException("File not found");
            }

            var fileStream = new FileStream(fileDocument.Path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            return (fileStream, fileDocument.ContentType, fileDocument.Size);
        }

        public async Task<bool> DeleteFileAsync(string id)
        {
            var fileDocument = await GetFileByIdAsync(id);
            if (fileDocument == null)
            {
                return false;
            }

            // Delete file from disk
            if (File.Exists(fileDocument.Path))
            {
                File.Delete(fileDocument.Path);
                
                // Remove parent directory if it's now empty
                var fileInfo = new FileInfo(fileDocument.Path);
                var parentDir = fileInfo.Directory;
                if (parentDir != null && !parentDir.EnumerateFiles().Any())
                {
                    parentDir.Delete();
                }
            }

            // Delete metadata from MongoDB
            var filter = Builders<FileDocument>.Filter.Eq(x => x.Id, id);
            var result = await _files.DeleteOneAsync(filter);

            _logger.LogInformation("File deleted successfully: {FileName} for service: {ServiceName}", fileDocument.OriginalFileName, fileDocument.ServiceName);

            return result.DeletedCount > 0;
        }
    }
}