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
        Task<ApiResponse<FileUploadResponse>> UploadAsync(IFormFile file, string token, CancellationToken ct = default);
        Task DeleteAsync(int fileId, string token, CancellationToken ct = default);
        Task<(byte[] Bytes, string ContentType)> DownloadAsync(int fileId, string token, CancellationToken ct = default);
    }
}
