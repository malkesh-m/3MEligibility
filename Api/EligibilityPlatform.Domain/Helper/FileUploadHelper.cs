using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Domain.Helper
{
    /// <summary>
    /// Helper class for managing file uploads to the wwwroot directory.
    /// </summary>
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Saves an uploaded file to the wwwroot/files/{tenantId}/ directory.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="webRootPath">The web root path (wwwroot).</param>
        /// <returns>The relative path to the saved file.</returns>
        public static async Task<string> SaveFileAsync(IFormFile file, int tenantId, string webRootPath)
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.");
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");
            }

            // Create directory if it doesn't exist
            var uploadDirectory = Path.Combine(webRootPath, "files", tenantId.ToString());
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDirectory, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            return $"files/{tenantId}/{uniqueFileName}";
        }

        /// <summary>
        /// Deletes a file from the file system.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <param name="webRootPath">The web root path (wwwroot).</param>
        /// <returns>True if the file was deleted, false otherwise.</returns>
        public static bool DeleteFile(string? relativePath, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var filePath = Path.Combine(webRootPath, relativePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the full file path from a relative path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="webRootPath">The web root path (wwwroot).</param>
        /// <returns>The full file path.</returns>
        public static string GetFullPath(string relativePath, string webRootPath)
        {
            return Path.Combine(webRootPath, relativePath);
        }
    }
}
