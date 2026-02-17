using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEligibilityPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MEligibilityPlatform.Application.Services.Interface
{
   public interface IDriveService
    {
        Task<ApiResponse<FileUploadResponse>> UploadAsync(IFormFile file, string token);
        Task DeleteAsync(int fileId, string token);
        Task<(byte[] Bytes, string ContentType)> DownloadAsync(int fileId, string token);
    }
}
