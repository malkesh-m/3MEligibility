using Microsoft.AspNetCore.Http;

namespace EligibilityPlatform.Domain.Helper
{
    public class ImageUploadHelper
    {
        private const int MaxFileSize = 5 * 1024 * 1024;
        private readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];

        /// <summary>
        /// Processes an uploaded image file and converts it into a byte array.
        /// </summary>
        /// <param name="file">The uploaded image file.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description><c>imageBytes</c> - The image content as a byte array, or <c>null</c> if the file is invalid.</description></item>
        ///   <item><description><c>mimeType</c> - The MIME type of the uploaded file, or <c>null</c> if the file is invalid.</description></item>
        /// </list>
        /// </returns>
        public static async Task<(byte[]? imageBytes, string? mimeType)> ProcessImageUpload(IFormFile file)
        {
            // Validates that the file is not null and contains data
            if (file == null || file.Length == 0)
                return (null, null);

            // Creates a memory stream to temporarily store the uploaded file content
            using var memoryStream = new MemoryStream();
            // Asynchronously copies the file content from the IFormFile to the memory stream
            await file.CopyToAsync(memoryStream);
            // Converts the memory stream content to a byte array for database storage
            byte[]? imageBytes = memoryStream.ToArray();
            // Returns both the image byte array and the original file's MIME type for proper content handling
            return (memoryStream.ToArray(), file.ContentType);
        }

        
    }
}
